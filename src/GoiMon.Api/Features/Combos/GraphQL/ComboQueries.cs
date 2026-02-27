using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType("Query")]
public class ComboQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UsePaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<ProductCombo> Combos([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.ProductCombos.Include(c => c.Items).AsQueryable();

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> ComboById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
}
