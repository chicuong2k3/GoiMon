using Hangfire;
using Hangfire.PostgreSql;
using GoiMon.Api.Infrastructure.Outbox;
using GoiMon.Api.Infrastructure.Services;
using GoiMon.Api.Features.Authentication.Mutations;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using FluentValidation;
using GoiMon.Api.Features.Products;
using GoiMon.Api.Features.Categories;
using GoiMon.Api.Features.Orders;
using GoiMon.Api.Features.Combos;
using CloudinaryDotNet;
using GoiMon.Api.Features.ImageUpload.Services;
using GoiMon.Api.Features.ImageUpload.Mutations;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console(
            theme: AnsiConsoleTheme.Code,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
});

// Configuration
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

// EF Core
builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(conn));

// CORS: allow requests from Nitro app and local dev origins
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNitro", policy =>
    {
        policy
            .WithOrigins("https://nitro.chillicream.com", "http://localhost:5002", "http://localhost:5003", "http://localhost:5000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
    // Development helper: a permissive policy for local development (use with caution)
    options.AddPolicy("AllowLocalDev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// FluentValidation: register validators from this assembly
builder.Services.AddValidatorsFromAssemblyContaining<GoiMon.Api.Features.Categories.Validators.CreateCategoryInputValidator>();

// Authentication Services
builder.Services.AddHttpClient<IOAuthExchangeService, OAuthExchangeService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddSingleton<IJwtTokenService, JwtTokenService>();

// Cloudinary Image Upload
var cloudinaryAccount = new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]);
builder.Services.AddSingleton(new Cloudinary(cloudinaryAccount));
builder.Services.AddScoped<IImageUploadService, CloudinaryImageUploadService>();

// MediatR removed — validation is handled via FluentValidation middleware and error filter

// Bridge: HC's ServiceKind.Pooled resolves ObjectPool<T> from DI at request time.
// AddPooledDbContextFactory only registers IDbContextFactory<T>, so we wrap it here.
builder.Services.AddSingleton<ObjectPool<AppDbContext>>(sp =>
    new PooledDbContextObjectPool<AppDbContext>(
        sp.GetRequiredService<IDbContextFactory<AppDbContext>>()));

// Register the outbox worker as an injectable service (invoked by the chosen background processor)
builder.Services.AddTransient<OutboxService>();

// Hangfire: dashboard + background server using PostgreSQL storage (use a dedicated schema)
builder.Services.AddHangfire(cfg => cfg.UsePostgreSqlStorage(conn, new Hangfire.PostgreSql.PostgreSqlStorageOptions
{
    SchemaName = "hangfire"
}));
builder.Services.AddHangfireServer();

// Domain event dispatcher and handlers
builder.Services.AddScoped<GoiMon.Api.Domain.Events.IDomainEventDispatcher, GoiMon.Api.Infrastructure.DomainEvents.DomainEventDispatcher>();
builder.Services.AddScoped(typeof(GoiMon.Api.Domain.Events.IDomainEventHandler<>), typeof(GoiMon.Api.Infrastructure.DomainEvents.LoggingDomainEventHandler<>));

// GraphQL (HotChocolate)
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true)
    // Register aggregate-specific extensions
    .AddTypeExtension<ProductQueries>()
    .AddTypeExtension<ProductMutations>()
    .AddTypeExtension<OrderMutations>()
    .AddTypeExtension<ProductResolvers>()
    .AddTypeExtension<CategoryQueries>()
    .AddTypeExtension<CategoryMutations>()
    .AddTypeExtension<ComboQueries>()
    .AddTypeExtension<ComboMutations>()
    .AddTypeExtension<ProductComboItemResolvers>()
    .AddTypeExtension<AuthenticationMutations>()
    .AddTypeExtension<ImageUploadMutations>()
    .AddErrorFilter<GoiMon.Api.Infrastructure.Validation.FluentValidationErrorFilter>()
    // Validate input objects via FluentValidation middleware
    .UseField<GoiMon.Api.Infrastructure.Validation.FluentValidationMiddleware>();

// Repositories currently unused by GraphQL resolvers (using DB-backed resolvers instead).
// If you later want to reintroduce repository abstractions, register them here.

var app = builder.Build();

// Apply EF Core migrations at startup and seed data
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    try
    {
        using var db = factory.CreateDbContext();
        // Use Migrate for proper schema evolution (requires migrations to be created via dotnet-ef)
        db.Database.Migrate();
        // Seed initial data
        await SeedData.SeedAsync(db);
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Database migrate/seed error");
    }
}

app.MapGet("/health", () => Results.Ok("ok"));

// Enable CORS for browser clients (must be before endpoints that serve requests)
app.UseCors("AllowNitro");

// Hangfire dashboard and recurring job for outbox processing
// Expose the dashboard at /hangfire (no auth configured here — restrict in production)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = Array.Empty<Hangfire.Dashboard.IDashboardAuthorizationFilter>()
});

RecurringJob.AddOrUpdate<OutboxService>(
    "outbox-poll",
    s => s.ProcessPendingAsync(CancellationToken.None),
    Cron.Minutely);

app.MapGraphQL();

try
{
    Log.Information("Starting web host");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Adapts IDbContextFactory&lt;T&gt; as ObjectPool&lt;T&gt; so HotChocolate's ServiceKind.Pooled
/// can resolve the DbContext from the DI container.
/// Get() rents a context from EF Core's internal pool; Return() disposes it back.
/// </summary>
file sealed class PooledDbContextObjectPool<TContext>(IDbContextFactory<TContext> factory)
    : ObjectPool<TContext> where TContext : DbContext
{
    public override TContext Get() => factory.CreateDbContext();
    public override void Return(TContext obj) => obj.Dispose();
}
