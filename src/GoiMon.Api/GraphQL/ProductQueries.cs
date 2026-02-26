using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType("Query")]
public class ProductQueries
{
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
    public IQueryable<Product> ProductsByCategory(string category, [Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Products.Where(p => p.Category == category).AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product?> ProductById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Products.FirstOrDefaultAsync(p => p.Id == id);
}
