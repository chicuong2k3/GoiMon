namespace GoiMon.Api.Features.Tables.Queries;

[ExtendObjectType("Query")]
public sealed class TableQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<TableSlot> TableSlots([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.TableSlots.AsQueryable();
}
