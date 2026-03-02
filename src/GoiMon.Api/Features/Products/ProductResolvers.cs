using GoiMon.Api.Features.Categories;

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

    [UseDbContext(typeof(AppDbContext))]
    public async Task<List<ProductVariant>> GetVariantsAsync(
        [Parent] Product product,
        [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.ProductVariants
            .Where(v => v.ProductId == product.Id)
            .OrderBy(v => v.SortOrder)
            .ToListAsync();

    [UseDbContext(typeof(AppDbContext))]
    public async Task<List<ModifierGroup>> GetModifierGroupsAsync(
        [Parent] Product product,
        [Service(ServiceKind.Pooled)] AppDbContext db)
        => await db.ModifierGroups
            .Include(g => g.Options)
            .Where(g => g.ProductId == product.Id)
            .OrderBy(g => g.SortOrder)
            .ToListAsync();
}
