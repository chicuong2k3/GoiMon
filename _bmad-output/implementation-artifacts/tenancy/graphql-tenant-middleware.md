# GraphQL Tenant Middleware (ASP.NET Core + HotChocolate) — .NET 8

Purpose: enforce tenant isolation by extracting `tenant_id` from the authenticated JWT (or a tenant header), validating it, and making it available to GraphQL resolvers via request context.

1) Minimal ASP.NET Core middleware (sets `HttpContext.Items["TenantId"]`)

```csharp
// TenantMiddleware.cs
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        // Prefer JWT claim, fallback to header
        string tenantId = context.User?.FindFirst("tenant_id")?.Value
                          ?? context.Request.Headers["x-tenant-id"].FirstOrDefault();

        if (string.IsNullOrEmpty(tenantId))
        {
            // Optionally: reject anonymous requests that require tenant
            // context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            // return;
        }

        context.Items["TenantId"] = tenantId;
        await _next(context);
    }
}

// Register in Program.cs
// app.UseMiddleware<TenantMiddleware>();
```

2) HotChocolate: add an HTTP request interceptor to copy tenant into GraphQL context

```csharp
// In Program.cs when configuring GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    // ... types
    .AddHttpRequestInterceptor(async (context, executor, requestBuilder, ct) =>
    {
        var httpContext = context.GetHttpContext();
        if (httpContext.Items.TryGetValue("TenantId", out var t) && t is string tenantId)
        {
            // store in GraphQL context data so resolvers can access
            context.ContextData["tenantId"] = tenantId;
        }
        await Task.CompletedTask;
    });

// Resolver example
public class OrderResolver
{
    public Task<Order[]> GetOrders([Service] IDbConnection db, IResolverContext ctx)
    {
        var tenantId = ctx.ContextData["tenantId"] as string;
        // Use tenantId in queries (parameterized)
        // SELECT * FROM orders WHERE tenant_id = @tenantId ...
    }
}
```

Notes
- Enforce tenant checks at middleware and DB layer (never trust client-provided tenant header alone).
- Use JWT with `tenant_id` claim generated at onboarding; rotate tenant keys for sync tokens.
- For high security tenants consider audience/issuer and per-tenant signing keys.
