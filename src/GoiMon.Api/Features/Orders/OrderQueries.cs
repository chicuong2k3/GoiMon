namespace GoiMon.Api.Features.Orders;

[ExtendObjectType("Query")]
public class OrderQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UsePaging(IncludeTotalCount = true, MaxPageSize = 50)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Order> Orders([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Orders.AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> OrderById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
}
