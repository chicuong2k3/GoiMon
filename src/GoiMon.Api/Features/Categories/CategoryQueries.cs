namespace GoiMon.Api.Features.Categories;

[ExtendObjectType("Query")]
public class CategoryQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Category> Categories([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Categories.AsQueryable();

    // Offset (index) paging variant: clients can request by page number/size.
    [UseDbContext(typeof(AppDbContext))]
    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Category> CategoriesOffset([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Categories.AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Category?> CategoryById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
}
