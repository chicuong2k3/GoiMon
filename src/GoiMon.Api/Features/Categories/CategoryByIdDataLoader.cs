using Microsoft.EntityFrameworkCore;
using GoiMon.Api.Infrastructure.Data;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.Features.Categories;

public class CategoryByIdDataLoader : BatchDataLoader<Guid, Category>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public CategoryByIdDataLoader(
        IBatchScheduler batchScheduler,
        IDbContextFactory<AppDbContext> dbFactory)
        : base(batchScheduler)
    {
        _dbFactory = dbFactory;
    }

    protected override async Task<IReadOnlyDictionary<Guid, Category>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        await using var db = _dbFactory.CreateDbContext();
        var items = await db.Categories.Where(c => keys.Contains(c.Id)).ToListAsync(cancellationToken);
        return items.ToDictionary(c => c.Id);
    }
}
