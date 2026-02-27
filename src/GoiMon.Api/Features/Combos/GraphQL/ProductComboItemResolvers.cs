using HotChocolate;
using HotChocolate.Types;
using GoiMon.Api.DataLoaders;
using System.Threading;
using System.Threading.Tasks;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType(typeof(ProductComboItem))]
public class ProductComboItemResolvers
{
    public async Task<Product?> GetProductAsync(
        [Parent] ProductComboItem item,
        ProductByIdDataLoader loader,
        CancellationToken ct)
    {
        return await loader.LoadAsync(item.ProductId, ct);
    }
}
