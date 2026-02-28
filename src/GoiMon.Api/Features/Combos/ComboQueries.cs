namespace GoiMon.Api.Features.Combos;

[ExtendObjectType("Query")]
public class ComboQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProductCombo> Combos([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.ProductCombos.Include(c => c.Items).AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> ComboById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
}
