using GoiMon.Api.Domain.Entities;
using GoiMon.Api.Data;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType("Mutation")]
public class OrderMutations
{
    [UseDbContext(typeof(AppDbContext))]
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

public record OrderInput(List<OrderItemInput> Items);
public record OrderItemInput(Guid ProductId, int Qty, int UnitPriceCents);
