using HotChocolate;
using GoiMon.Api.DataLoaders;
using GoiMon.Api.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType(typeof(Product))]
public class ProductResolvers
{
    public async Task<Category?> GetCategoryAsync(
        [Parent] Product product,
        CategoryByIdDataLoader loader,
        CancellationToken ct)
    {
        if (product.CategoryId == Guid.Empty) return null;
        return await loader.LoadAsync(product.CategoryId, ct);
    }
}
