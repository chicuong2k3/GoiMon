using GoiMon.Api.Features.Categories;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.Features.Products;

[ExtendObjectType(typeof(Product))]
public class ProductResolvers
{
    public async Task<Category?> GetCategoryAsync(
        [Parent] Product product,
        CategoryByIdDataLoader loader,
        CancellationToken ct)
    {
        if (!product.CategoryId.HasValue) return null;
        return await loader.LoadAsync(product.CategoryId.Value, ct);
    }
}
