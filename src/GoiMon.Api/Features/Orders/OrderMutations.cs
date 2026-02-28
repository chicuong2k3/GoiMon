using GoiMon.Api.Domain.Entities;
using GoiMon.Api.Data;

namespace GoiMon.Api.GraphQL;

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
    public async Task<Order> CreateOrder(OrderInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var order = new Order(Guid.NewGuid());

        foreach (var it in input.Items)
        {
            order.AddItem(it.ProductId, it.Qty, it.UnitPriceCents);
        }

        db.Orders.Add(order);
        await db.SaveChangesAsync();
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
/// </summary>
/// <param name="ProductId">Referenced product identifier.</param>
/// <param name="Qty">Quantity of the product.</param>
/// <param name="UnitPriceCents">Unit price in cents.</param>
public record OrderItemInput(Guid ProductId, int Qty, int UnitPriceCents);
