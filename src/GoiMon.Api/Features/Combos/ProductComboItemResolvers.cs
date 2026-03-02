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

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductVariant?> GetVariantAsync(
        [Parent] ProductComboItem item,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        CancellationToken ct)
    {
        if (!item.VariantId.HasValue)
        {
            return null;
        }

        return await db.ProductVariants.FirstOrDefaultAsync(v => v.Id == item.VariantId.Value, ct);
    }
}
