using GoiMon.Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration
var conn = builder.Configuration.GetConnectionString("DefaultConnection");

// EF Core
builder.Services.AddPooledDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(conn));

// GraphQL (HotChocolate)
builder.Services
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
    .AddMutationType(d => d.Name("Mutation"))
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    // Enable Relay-style global object identification (Node interface, global IDs)
    .AddGlobalObjectIdentification()
    // Register aggregate-specific extensions
    .AddTypeExtension<GoiMon.Api.GraphQL.ProductQueries>()
    .AddTypeExtension<GoiMon.Api.GraphQL.OrderQueries>()
    .AddTypeExtension<GoiMon.Api.GraphQL.ProductMutations>()
    .AddTypeExtension<GoiMon.Api.GraphQL.OrderMutations>()
    .AddTypeExtension<GoiMon.Api.GraphQL.ProductResolvers>()
    .AddTypeExtension<GoiMon.Api.GraphQL.ComboQueries>()
    .AddTypeExtension<GoiMon.Api.GraphQL.ComboMutations>()
    .AddTypeExtension<GoiMon.Api.GraphQL.ProductComboItemResolvers>()
        .AddTypeExtension<GoiMon.Api.GraphQL.CategoryQueries>()
        .AddTypeExtension<GoiMon.Api.GraphQL.CategoryMutations>()
    .AddType<GoiMon.Api.GraphQL.Types.ProductCategory>();

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
        // Log or ignore for now; surface minimal info to console for developer
        Console.WriteLine($"Database migrate/seed error: {ex.Message}");
    }
}

app.MapGet("/health", () => Results.Ok("ok"));

app.MapGraphQL();

app.Run();
