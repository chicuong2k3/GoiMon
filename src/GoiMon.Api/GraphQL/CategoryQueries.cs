using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.GraphQL;

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

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Category?> CategoryById(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.Categories.FirstOrDefaultAsync(c => c.Id == id);
}
