# Program.cs wiring example (ASP.NET Core, .NET 8)

Pastes for `Program.cs` showing authentication, tenant middleware, HotChocolate GraphQL wiring, and GraphQlTenantInterceptor registration.

```csharp
var builder = WebApplication.CreateBuilder(args);

// Typical services
builder.Services.AddAuthentication(/* your auth config */);
builder.Services.AddAuthorization();

// Add GraphQL server and register the tenant interceptor
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    // register the interceptor that copies tenant into GraphQL context
    .AddHttpRequestInterceptor< GoiMon.Tenancy.GraphQlTenantInterceptor >();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Tenant resolution middleware should run after authentication so JWT claims are available
app.UseTenantResolution();

// Map GraphQL endpoint
app.MapGraphQL();

app.Run();
```

Resolver access example (in a resolver):

```csharp
public class OrderResolver
{
    public Task<Order[]> GetOrders(IResolverContext ctx)
    {
        // tenantId is available via context.ContextData or request property
        var tenantId = ctx.ContextData.TryGetValue("tenantId", out var t) ? t as string : null;
        // use tenantId in queries to ensure isolation
    }
}
```

Notes:
- Ensure `TenantMiddleware` is registered (app.UseTenantResolution()) after authentication so the middleware can read JWT claims.
- Always enforce `tenant_id` at DB/query level (include WHERE tenant_id = @tenantId) to avoid accidental cross-tenant leakage.
