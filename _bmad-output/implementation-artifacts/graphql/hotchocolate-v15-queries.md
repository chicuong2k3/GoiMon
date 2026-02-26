HotChocolate v15 — Queries Guide (GoiMon)

Goal: demonstrate HotChocolate v15 code-first query patterns and when to use them.

1) Two common resolver styles
- Repository-returning resolvers (safe, testable)
  - Example: `GetProducts([Service] IProductRepository repo)` returns `Task<List<Product>>`.
  - Pros: decouples persistence, easy to unit test domain logic and repositories.
  - Cons: cannot leverage HotChocolate's projection/filtering/sorting middleware to translate to SQL.

- IQueryable (DB-backed) resolvers (v15 pattern)
  - Example: `GetProductsDb([ScopedService] AppDbContext db)` returns `IQueryable<Product>`.
  - Decorate with: `[UseDbContext(typeof(AppDbContext))]`, `[UseProjection]`, `[UseFiltering]`, `[UseSorting]`.
  - Pros: HotChocolate applies projections/filters/sorts at the database level (efficient SQL), supports nested projections.
  - Cons: ties GraphQL layer to EF Core; harder to unit-test without an in-memory provider or abstraction.

2) How it's implemented in this repo
- File: `src/GoiMon.Api/GraphQL/Query.cs` contains both styles:
  - `GetProducts` / `GetOrders` use repository interfaces.
  - `GetProductsDb` / `GetOrdersDb` return `IQueryable<>` and use the `[UseDbContext]`, `[UseProjection]`, `[UseFiltering]`, `[UseSorting]` attributes so HotChocolate v15 runs database-side queries.
- `Program.cs` already registers HotChocolate features: `.AddProjections().AddFiltering().AddSorting()`.

3) When to choose which
- Use repository-returning resolvers when you need strict separation, complex domain logic inside repositories/services, or prefer easier unit testing.
- Use IQueryable resolvers when performance matters and you need server-side projection/filtering of large datasets.

4) Practical tips
- For IQueryable resolvers, return `IQueryable<T>` not `IEnumerable<T>` or `Task<T>` so HotChocolate can build expression trees.
- Use `Include(...)` only when nested navigation properties are required; prefer `UseProjection` to let HotChocolate determine necessary includes.
- Protect sensitive fields with authorization policies (e.g., `[Authorize]`) at resolver or schema level.
- For multi-tenant apps, add a global query filter on `DbContext` (e.g., `modelBuilder.Entity<Product>().HasQueryFilter(p => p.TenantId == _currentTenantId)`).

5) Example query
```
query {
  productsDb(where: { priceCents: { gt: 30000 } }, order: { priceCents: DESC }) {
    id
    name
    priceCents
    category
  }
}
```

6) Next steps
- If you want full schema examples, I can add a small GraphQL client (`Banana Cake Pop`) compatible playground or generate schema SDL and sample queries.
- I can also convert resolvers to `ObjectType` classes and show registering explicit types if you prefer more control.

