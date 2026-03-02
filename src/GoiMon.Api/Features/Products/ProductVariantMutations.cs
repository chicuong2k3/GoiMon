namespace GoiMon.Api.Features.Products;

[ExtendObjectType("Mutation")]
public class ProductVariantMutations
{
    // ── ProductVariant ──────────────────────────────────────────────────────

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductVariant> CreateProductVariant(
        Guid productId,
        ProductVariantInput input,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var variant = new ProductVariant(
            Guid.NewGuid(), productId,
            input.Code, input.Name, input.Price,
            input.SortOrder, input.IsActive);
        db.ProductVariants.Add(variant);
        await db.SaveChangesAsync();
        return variant;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductVariant?> UpdateProductVariant(
        Guid id,
        ProductVariantInput input,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var variant = await db.ProductVariants.FindAsync(id);
        if (variant is null) return null;

        variant.UpdateName(input.Name);
        variant.UpdatePrice(input.Price);
        variant.UpdateSortOrder(input.SortOrder);
        variant.SetActive(input.IsActive);
        await db.SaveChangesAsync();
        return variant;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteProductVariant(
        Guid id,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var variant = await db.ProductVariants.FindAsync(id);
        if (variant is null) return false;
        db.ProductVariants.Remove(variant);
        await db.SaveChangesAsync();
        return true;
    }

    // ── ModifierGroup ───────────────────────────────────────────────────────

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ModifierGroup> CreateModifierGroup(
        Guid productId,
        ModifierGroupInput input,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var group = new ModifierGroup(
            Guid.NewGuid(), productId,
            input.Name, input.SelectionMode,
            input.MinSelect, input.MaxSelect,
            input.SortOrder, input.IsRequired, input.IsActive);
        db.ModifierGroups.Add(group);
        await db.SaveChangesAsync();
        return group;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ModifierGroup?> UpdateModifierGroup(
        Guid id,
        ModifierGroupInput input,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var group = await db.ModifierGroups.FindAsync(id);
        if (group is null) return null;

        group.UpdateName(input.Name);
        group.UpdateSelection(input.SelectionMode, input.MinSelect, input.MaxSelect);
        group.SetActive(input.IsActive);
        await db.SaveChangesAsync();
        return group;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteModifierGroup(
        Guid id,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var group = await db.ModifierGroups
            .Include(g => g.Options)
            .FirstOrDefaultAsync(g => g.Id == id);
        if (group is null) return false;
        db.ModifierGroups.Remove(group);
        await db.SaveChangesAsync();
        return true;
    }

    // ── ModifierOption ──────────────────────────────────────────────────────

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ModifierOption> CreateModifierOption(
        Guid groupId,
        ModifierOptionInput input,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var option = new ModifierOption(
            Guid.NewGuid(), groupId,
            input.Name, input.PriceDelta,
            input.MaxQty, input.SortOrder,
            input.IsDefault, input.IsActive);
        db.ModifierOptions.Add(option);
        await db.SaveChangesAsync();
        return option;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ModifierOption?> UpdateModifierOption(
        Guid id,
        ModifierOptionInput input,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var option = await db.ModifierOptions.FindAsync(id);
        if (option is null) return null;

        option.UpdateName(input.Name);
        option.UpdatePriceDelta(input.PriceDelta);
        option.UpdateMaxQty(input.MaxQty);
        option.SetActive(input.IsActive);
        await db.SaveChangesAsync();
        return option;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteModifierOption(
        Guid id,
        [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var option = await db.ModifierOptions.FindAsync(id);
        if (option is null) return false;
        db.ModifierOptions.Remove(option);
        await db.SaveChangesAsync();
        return true;
    }
}

public record ProductVariantInput(
    string Code,
    string Name,
    decimal Price,
    int SortOrder = 0,
    bool IsActive = true);

public record ModifierGroupInput(
    string Name,
    ModifierSelectionMode SelectionMode,
    int MinSelect,
    int MaxSelect,
    int SortOrder = 0,
    bool IsRequired = false,
    bool IsActive = true);

public record ModifierOptionInput(
    string Name,
    decimal PriceDelta,
    int MaxQty = 1,
    int SortOrder = 0,
    bool IsDefault = false,
    bool IsActive = true);
