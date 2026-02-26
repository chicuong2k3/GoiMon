using System.Collections.Concurrent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using HotChocolate;
using HotChocolate.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddHttpRequestInterceptor<GraphQlTenantInterceptor>();

var app = builder.Build();

// Simple tenant middleware that reads x-tenant-id header and stores it in HttpContext.Items
app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue("x-tenant-id", out var t))
    {
        context.Items["TenantId"] = t.ToString();
    }
    await next();
});

app.MapGraphQL();

// Playground for manual testing
app.UsePlayground();

app.Run();

// ----------------- Supporting types (kept here for demo simplicity) -----------------

public class GraphQlTenantInterceptor : IHttpRequestInterceptor
{
    public ValueTask OnCreateAsync(HttpContext httpContext, IRequestExecutor requestExecutor, IQueryRequestBuilder requestBuilder, CancellationToken cancellationToken)
    {
        if (httpContext is not null && httpContext.Items.TryGetValue("TenantId", out var t) && t is string tenantId)
        {
            requestBuilder.SetProperty("tenantId", tenantId);
        }
        return ValueTask.CompletedTask;
    }
}

public static class InMemoryStore
{
    // tenantId -> items
    public static ConcurrentDictionary<string, List<Item>> ItemsByTenant { get; } = new();
    // tenantId -> orders
    public static ConcurrentDictionary<string, List<Order>> OrdersByTenant { get; } = new();

    static InMemoryStore()
    {
        // Seed example items for demo tenants
        ItemsByTenant["tenant-demo"] = new List<Item>
        {
            new Item { ItemId = "i1", Name = "Pho", PriceCents = 30000 },
            new Item { ItemId = "i2", Name = "Banh Mi", PriceCents = 20000 }
        };
    }
}

public class Query
{
    public IEnumerable<Item> GetItems(IResolverContext ctx)
    {
        var tenantId = ctx.ContextData.TryGetValue("tenantId", out var t) ? t as string : null;
        if (tenantId is null) return Enumerable.Empty<Item>();
        if (InMemoryStore.ItemsByTenant.TryGetValue(tenantId, out var list)) return list;
        return Enumerable.Empty<Item>();
    }
}

public class Mutation
{
    public Order CreateOrder(OrderInput input, IResolverContext ctx)
    {
        var tenantId = ctx.ContextData.TryGetValue("tenantId", out var t) ? t as string : null;
        tenantId ??= "tenant-demo";

        var order = new Order
        {
            OrderId = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Items = input.Items,
            TotalCents = input.Items.Sum(i => i.UnitPriceCents * i.Qty),
            Status = "paid"
        };

        var list = InMemoryStore.OrdersByTenant.GetOrAdd(tenantId, _ => new List<Order>());
        list.Add(order);
        return order;
    }
}

public record Item
{
    public string ItemId { get; init; } = default!;
    public string Name { get; init; } = default!;
    public int PriceCents { get; init; }
}

public record Order
{
    public string OrderId { get; init; } = default!;
    public string TenantId { get; init; } = default!;
    public List<OrderLine> Items { get; init; } = new();
    public int TotalCents { get; init; }
    public string Status { get; init; } = "open";
}

public record OrderInput
{
    public List<OrderLine> Items { get; init; } = new();
}

public record OrderLine
{
    public string ItemId { get; init; } = default!;
    public int Qty { get; init; }
    public int UnitPriceCents { get; init; }
}
