using Microsoft.EntityFrameworkCore;
using GoiMon.Api.Infrastructure.Data;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.Features.Products;

/// <summary>
/// Batch DataLoader to load <see cref="Product"/> by Id and avoid N+1 queries.
/// </summary>
public class ProductByIdDataLoader : BatchDataLoader<Guid, Product>
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;

    public ProductByIdDataLoader(
        IBatchScheduler batchScheduler,
        IDbContextFactory<AppDbContext> dbFactory)
        : base(batchScheduler)
    {
        _dbFactory = dbFactory;
    }

    protected override async Task<IReadOnlyDictionary<Guid, Product>> LoadBatchAsync(
        IReadOnlyList<Guid> keys,
        CancellationToken cancellationToken)
    {
        await using var db = _dbFactory.CreateDbContext();
        var products = await db.Products.Where(p => keys.Contains(p.Id)).ToListAsync(cancellationToken);
        return products.ToDictionary(p => p.Id);
    }
}
