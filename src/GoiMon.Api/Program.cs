using Hangfire;
using Hangfire.PostgreSql;
using GoiMon.Api.Infrastructure.Outbox;
using GoiMon.Api.Infrastructure.Services;
using GoiMon.Api.Infrastructure.Telemetry;
using GoiMon.Api.Infrastructure.Middleware;
using GoiMon.Api.Features.Authentication.Mutations;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using FluentValidation;
using GoiMon.Api.Features.Products;
using GoiMon.Api.Features.Categories;
using GoiMon.Api.Features.Orders;
using GoiMon.Api.Features.Combos;
using GoiMon.Api.Features.Employees.Queries;
using GoiMon.Api.Features.Employees.Mutations;
using GoiMon.Api.Features.Tables.Queries;
using GoiMon.Api.Features.Tables.Mutations;
using GoiMon.Api.Features.Tables.Subscriptions;
using CloudinaryDotNet;
using Npgsql;
using GoiMon.Api.Features.ImageUpload.Services;

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
            .WithOrigins(
                "https://nitro.chillicream.com",
                "http://localhost:5000",
                "http://localhost:5001",
                "http://localhost:5002",
                "http://localhost:5003",
                "http://45.115.16.61",
                "https://45.115.16.61")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
    // Development helper: a permissive policy for local development (use with caution)
    options.AddPolicy("AllowLocalDev", p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

// FluentValidation: register validators from this assemblyl
builder.Services.AddValidatorsFromAssemblyContaining<GoiMon.Api.Features.Categories.Validators.CreateCategoryInputValidator>();

// Authentication & Authorization Services
var jwtKey = builder.Configuration["Jwt:SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey missing");
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "goimon-api",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "goimon-client",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

GoiMon.Api.Infrastructure.Authorization.AuthorizationConfig.AddPolicyMatrix(builder.Services);

// Authentication Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, HttpContextTenantProvider>();
builder.Services.AddSingleton<ITenantAccessor, TenantAccessor>();
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
builder.Services.AddSingleton<GoiMon.Api.Features.Orders.Services.IOrderTelemetry, GoiMon.Api.Features.Orders.Services.OrderTelemetry>();
builder.Services.AddSingleton<GoiMon.Api.Infrastructure.Telemetry.IPosOperationTelemetry, GoiMon.Api.Infrastructure.Telemetry.PosOperationTelemetry>();

// MediatR removed — validation is handled via FluentValidation middleware and error filter

// Bridge: HC's ServiceKind.Pooled resolves ObjectPool<T> from DI at request time.
// AddPooledDbContextFactory only registers IDbContextFactory<T>, so we wrap it here.
builder.Services.AddSingleton<ObjectPool<AppDbContext>>(sp =>
    new PooledDbContextObjectPool<AppDbContext>(
        sp.GetRequiredService<IDbContextFactory<AppDbContext>>()));

// Register the outbox worker as an injectable service (invoked by the chosen background processor)
builder.Services.AddTransient<OutboxService>();

// Hangfire: only enable if the database is reachable. In dev we want the app to start
// even when Postgres isn't running (useful for local iteration without DB).
var _dbReachable = true;
try
{
    using var _tmp = new NpgsqlConnection(conn);
    _tmp.Open();
    _tmp.Close();
}
catch (Exception ex)
{
    _dbReachable = false;
    Log.Warning(ex, "Database not reachable at startup; skipping Hangfire registration (Development/CI-friendly)");
}

if (_dbReachable)
{
    builder.Services.AddHangfire(cfg => cfg.UsePostgreSqlStorage(conn, new Hangfire.PostgreSql.PostgreSqlStorageOptions
    {
        SchemaName = "hangfire"
    }));
    builder.Services.AddHangfireServer();
}

// Domain event dispatcher and handlers
builder.Services.AddScoped<GoiMon.Api.Domain.Events.IDomainEventDispatcher, GoiMon.Api.Infrastructure.DomainEvents.DomainEventDispatcher>();
builder.Services.AddScoped(typeof(GoiMon.Api.Domain.Events.IDomainEventHandler<>), typeof(GoiMon.Api.Infrastructure.DomainEvents.LoggingDomainEventHandler<>));
builder.Services.AddScoped<GoiMon.Api.Domain.Events.IDomainEventHandler<GoiMon.Api.Domain.Events.ProductImageUpdatedEvent>, GoiMon.Api.Infrastructure.DomainEvents.ImageCleanupHandler>();
builder.Services.AddScoped<GoiMon.Api.Domain.Events.IDomainEventHandler<GoiMon.Api.Domain.Events.ProductDeletedEvent>, GoiMon.Api.Infrastructure.DomainEvents.ImageCleanupHandler>();
builder.Services.AddScoped<GoiMon.Api.Domain.Events.IDomainEventHandler<GoiMon.Api.Domain.Events.ComboImageUpdatedEvent>, GoiMon.Api.Infrastructure.DomainEvents.ImageCleanupHandler>();
builder.Services.AddScoped<GoiMon.Api.Domain.Events.IDomainEventHandler<GoiMon.Api.Domain.Events.ComboDeletedEvent>, GoiMon.Api.Infrastructure.DomainEvents.ImageCleanupHandler>();

// GraphQL (HotChocolate)
builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    .AddSubscriptionType(d => d.Name("Subscription"))
    .AddType<UploadType>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .ModifyRequestOptions(o => o.IncludeExceptionDetails = true)
    // Register aggregate-specific extensions
    .AddTypeExtension<ProductQueries>()
    .AddTypeExtension<ProductMutations>()
    .AddTypeExtension<ProductVariantMutations>()
    .AddTypeExtension<OrderMutations>()
    .AddTypeExtension<OrderQueries>()
    .AddTypeExtension<OrderSubscriptions>()
    .AddTypeExtension<OrderItemResolvers>()
    .AddTypeExtension<ProductResolvers>()
    .AddTypeExtension<CategoryQueries>()
    .AddTypeExtension<CategoryMutations>()
    .AddTypeExtension<ComboQueries>()
    .AddTypeExtension<ComboMutations>()
    .AddTypeExtension<ProductComboItemResolvers>()
    .AddTypeExtension<EmployeeQueries>()
    .AddTypeExtension<EmployeeMutations>()
    .AddTypeExtension<TableQueries>()
    .AddTypeExtension<TableMutations>()
    .AddTypeExtension<TableSubscriptions>()
    .AddTypeExtension<AuthenticationMutations>()
    .AddErrorFilter<GoiMon.Api.Infrastructure.Validation.FluentValidationErrorFilter>()
    // Validate input objects via FluentValidation middleware
    .UseField<GoiMon.Api.Infrastructure.Validation.FluentValidationMiddleware>()
    .AddInMemorySubscriptions();

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

app.UseWebSockets();
app.UseCorrelationId();

// Enable CORS for browser clients (must be before endpoints that serve requests)
app.UseCors("AllowNitro");

app.UseAuthentication();
// Populate TenantAccessor from HttpContext.User after authentication
app.UseMiddleware<TenantContextMiddleware>();
app.UseAuthorization();

// Hangfire dashboard and recurring job for outbox processing
if (_dbReachable)
{
    // Expose the dashboard at /hangfire (no auth configured here — restrict in production)
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = Array.Empty<Hangfire.Dashboard.IDashboardAuthorizationFilter>()
    });

    try
    {
        RecurringJob.AddOrUpdate<OutboxService>(
            "outbox-poll",
            s => s.ProcessPendingAsync(CancellationToken.None),
            Cron.Minutely);
    }
    catch (Exception ex)
    {
        Log.Warning(ex, "Failed to register recurring outbox job (lock timeout). Job may already exist.");
    }
}
else
{
    Log.Warning("Hangfire and recurring outbox job skipped because the database was not reachable at startup.");
}

app.MapImageUploadEndpoints();
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
