using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType("Query")]
public class OrderQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Order> Orders([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Orders.Include(o => o.Items).AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> OrderById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
}
