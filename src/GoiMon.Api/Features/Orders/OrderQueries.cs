using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GoiMon.Api.DataLoaders;
using GoiMon.Shared.Models;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType("Query")]
public class OrderQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Order> Orders([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Orders.Include(o => o.Items).AsQueryable();

    // Field resolver to expose items enriched with product data via DataLoader
    [UseDbContext(typeof(AppDbContext))]
    public Task<IEnumerable<OrderItemDto>> Items([Parent] Order order, ProductByIdDataLoader loader, CancellationToken ct)
        => GetItemsWithProductAsync(order, loader, ct);

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Order?> OrderById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

    // Expose items with Product info using DataLoader to avoid N+1
    public async Task<IEnumerable<OrderItemDto>> GetItemsWithProductAsync([Parent] Order order, ProductByIdDataLoader loader, CancellationToken ct)
    {
        var tasks = order.Items.Select(async it =>
        {
            var product = await loader.LoadAsync(it.ProductId, ct);
            return new OrderItemDto
            {
                Id = it.Id,
                ProductId = it.ProductId,
                ProductName = product?.Name,
                Qty = it.Qty,
                UnitPriceCents = it.UnitPriceCents
            };
        });

        return await Task.WhenAll(tasks);
    }
}
