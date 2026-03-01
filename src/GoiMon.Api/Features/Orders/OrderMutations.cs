using HotChocolate.Subscriptions;

namespace GoiMon.Api.Features.Orders;

/// <summary>
/// GraphQL mutation extensions for order-related operations.
/// These resolvers are registered as extensions to the root Mutation type.
/// </summary>
[ExtendObjectType("Mutation")]
public class OrderMutations
{
    [UseDbContext(typeof(AppDbContext))]
    /// <summary>
    /// Create a new order with the provided items and persist it to the database.
    /// Returns the created <see cref="Order"/> instance.
    /// </summary>
    /// <param name="input">Input payload containing order line items.</param>
    /// <param name="db">Pooled EF Core <see cref="AppDbContext"/> instance resolved from DI.</param>
    public async Task<Order> CreateOrder(
        OrderInput input,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] ITopicEventSender eventSender)
    {
        var order = new Order(Guid.NewGuid());

        foreach (var it in input.Items)
        {
            order.AddItem(it.ProductId, it.ProductName, it.Qty, it.UnitPrice, it.UnitName);
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();
        await eventSender.SendAsync(OrderSubscriptionTopics.OrderChanged, order);
        return order;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> CompleteOrder(
        Guid orderId,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] ITopicEventSender eventSender)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return null;
        order.MarkCompleted();
        await db.SaveChangesAsync();
        await eventSender.SendAsync(OrderSubscriptionTopics.OrderChanged, order);
        return order;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> CancelOrder(
        Guid orderId,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] ITopicEventSender eventSender)
    {
        var order = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order is null) return null;
        order.Cancel();
        await db.SaveChangesAsync();
        await eventSender.SendAsync(OrderSubscriptionTopics.OrderChanged, order);
        return order;
    }
}

/// <summary>
/// Input payload for creating an order.
/// </summary>
/// <param name="Items">List of order line items.</param>
public record OrderInput(List<OrderItemInput> Items);

/// <summary>
/// Input payload representing a single order line item.
/// Caller must pass a point-in-time snapshot of price and name — the server never
/// re-reads the current product row to determine what was ordered.
/// </summary>
/// <param name="ProductId">Optional soft-reference to the product (for BI/analytics).</param>
/// <param name="ProductName">Snapshot of the product name at order time.</param>
/// <param name="Qty">Quantity ordered.</param>
/// <param name="UnitPrice">Price per unit at order time.</param>
/// <param name="UnitName">Snapshot of the product's unit label at order time (e.g. "phần", "ly").</param>
public record OrderItemInput(Guid? ProductId, string ProductName, int Qty, decimal UnitPrice, string? UnitName = null);
