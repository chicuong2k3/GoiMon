namespace GoiMon.Api.Features.Employees.Queries;

[ExtendObjectType("Query")]
public sealed class EmployeeQueries
{
    [UseDbContext(typeof(AppDbContext))]
    [UseOffsetPaging(IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> Employees([Service(ServiceKind.Pooled)] AppDbContext db)
        => db.Users.AsQueryable();
}
