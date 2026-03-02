namespace GoiMon.Api.Features.Products;

[ExtendObjectType("Query")]
public class ProductQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UseOffsetPaging(IncludeTotalCount = true, MaxPageSize = 500)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> Products([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Products.AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> ProductsByCategoryName(string categoryName, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var cat = db.Categories.FirstOrDefault(c => c.Name == categoryName);
        if (cat is null) return Enumerable.Empty<Product>().AsQueryable();
        return db.Products.Where(p => p.CategoryId == cat.Id).AsQueryable();
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product?> ProductById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Products.FirstOrDefaultAsync(p => p.Id == id);
}
