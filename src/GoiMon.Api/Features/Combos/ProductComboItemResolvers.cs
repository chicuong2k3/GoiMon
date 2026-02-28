using GoiMon.Api.Features.Products;

namespace GoiMon.Api.Features.Combos;

[ExtendObjectType(typeof(ProductComboItem))]
public class ProductComboItemResolvers
{
    public async Task<Product?> GetProductAsync(
        [Parent] ProductComboItem item,
        ProductByIdDataLoader loader,
        CancellationToken ct)
        => await loader.LoadAsync(item.ProductId, ct);
}
