using GoiMon.Api.Domain.Entities;
using GoiMon.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.GraphQL;

public class Query
{
    // DB-backed queries (IQueryable) - preferred for projection/filtering/sorting
    [UseDbContext(typeof(AppDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> Products([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Products.AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> ProductsByCategory(Guid categoryId, [Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Products.Where(p => p.CategoryId == categoryId).AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product?> ProductById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Products.FirstOrDefaultAsync(p => p.Id == id);

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
// Deprecated central Query - queries have been split per aggregate into `ProductQueries` and `OrderQueries`.
// See src/GoiMon.Api/GraphQL/ProductQueries.cs and src/GoiMon.Api/GraphQL/OrderQueries.cs
