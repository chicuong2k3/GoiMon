namespace GoiMon.Api.Features.Orders;

[ExtendObjectType(typeof(OrderItem))]
public class OrderItemResolvers
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<List<OrderItemModifier>> GetModifiersAsync(
        [Parent] OrderItem item,
        [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.OrderItemModifiers
            .Where(m => m.OrderItemId == item.Id)
            .ToListAsync();
}
