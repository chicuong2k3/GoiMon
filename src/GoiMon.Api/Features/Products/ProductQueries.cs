using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using GoiMon.Api.GraphQL.Types;

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
    public IQueryable<Product> ProductsByCategory(Guid categoryId, [Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Products.Where(p => p.CategoryId == categoryId).AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> ProductsByCategoryEnum(ProductCategory category, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var cat = db.Categories.FirstOrDefault(c => c.Name == category.ToString());
        if (cat is null) return Enumerable.Empty<Product>().AsQueryable();
        return db.Products.Where(p => p.CategoryId == cat.Id).AsQueryable();
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product?> ProductById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Products.FirstOrDefaultAsync(p => p.Id == id);
}
