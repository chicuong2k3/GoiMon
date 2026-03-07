using GoiMon.Api.Features.ImageUpload.Services;
using GoiMon.Api.Infrastructure.Services;

namespace GoiMon.Api.Features.Combos;

[ExtendObjectType("Mutation")]
public class ComboMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo> CreateCombo(
        CreateComboInput input,
        [GraphQLType(typeof(UploadType))] IFile? image,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageUpload)
    {
        var combo = new ProductCombo(Guid.NewGuid(), input.Name, input.Price);
        if (input.Items is not null)
        {
            foreach (var item in input.Items)
            {
                await ValidateComboItemInputAsync(item.ProductId, item.VariantId, db);
                combo.AddItem(item.ProductId, item.Qty, item.VariantId);
            }
        }

        if (image is not null)
        {
            await using var stream = image.OpenReadStream();
            var url = await imageUpload.UploadImageAsync(stream, image.Name, "combos");
            combo.UpdateImage(url);
        }
        else if (!string.IsNullOrEmpty(input.ImageUrl))
        {
            combo.UpdateImage(input.ImageUrl);
        }

        db.ProductCombos.Add(combo);
        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> UpdateCombo(
        UpdateComboInput input,
        [GraphQLType(typeof(UploadType))] IFile? image,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageUpload)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == input.Id);
        if (combo is null) return null;

        if (input.Name is not null)
            combo.UpdateName(input.Name);
        if (input.Price.HasValue)
            combo.UpdatePrice(input.Price.Value);

        if (image is not null)
        {
            await using var stream = image.OpenReadStream();
            var url = await imageUpload.UploadImageAsync(stream, image.Name, "combos");
            combo.UpdateImage(url);
        }
        else if (input.ImageUrl is not null)
        {
            combo.UpdateImage(string.IsNullOrEmpty(input.ImageUrl) ? null : input.ImageUrl);
        }

        // Old image cleanup is handled by ImageCleanupHandler via domain event
        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteCombo(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.FindAsync(id);
        if (combo is null) return false;
        combo.MarkDeleted(); // Raises ComboDeletedEvent → ImageCleanupHandler deletes image
        db.ProductCombos.Remove(combo);
        await db.SaveChangesAsync();
        return true;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> AddComboItem(AddComboItemInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == input.ComboId);
        if (combo is null) return null;

        await ValidateComboItemInputAsync(input.ProductId, input.VariantId, db);
        combo.AddItem(input.ProductId, input.Qty, input.VariantId);
        db.ProductComboItems.Add(combo.Items.Last());
        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> UpdateComboItem(UpdateComboItemInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == input.ComboId);
        if (combo is null) return null;
        combo.UpdateItem(input.ItemId, input.Qty);
        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> RemoveComboItem(RemoveComboItemInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == input.ComboId);
        if (combo is null) return null;
        var item = combo.Items.FirstOrDefault(i => i.Id == input.ItemId);
        if (item is not null)
            db.ProductComboItems.Remove(item);
        combo.RemoveItem(input.ItemId);
        await db.SaveChangesAsync();
        return combo;
    }

    private static async Task ValidateComboItemInputAsync(Guid productId, Guid? variantId, AppDbContext db)
    {
        var activeVariants = await db.ProductVariants
            .Where(v => v.ProductId == productId && v.IsActive)
            .Select(v => v.Id)
            .ToListAsync();

        if (activeVariants.Count > 0 && variantId is null)
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetCode("COMBO_ITEM_VARIANT_REQUIRED")
                    .SetMessage("Variant is required because product has active variants")
                    .Build());
        }

        if (variantId is null)
        {
            return;
        }

        if (!activeVariants.Contains(variantId.Value))
        {
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetCode("COMBO_ITEM_INVALID_VARIANT")
                    .SetMessage("Variant is invalid, inactive, or does not belong to the selected product")
                    .Build());
        }
    }
}

public record CreateComboInput(string Name, decimal Price, List<ComboItemInput>? Items, string? ImageUrl = null);
public record UpdateComboInput(Guid Id, string? Name, decimal? Price, string? ImageUrl = null);
public record ComboItemInput(Guid ProductId, int Qty, Guid? VariantId = null);
public record AddComboItemInput(Guid ComboId, Guid ProductId, int Qty, Guid? VariantId = null);
public record UpdateComboItemInput(Guid ComboId, Guid ItemId, int Qty);
public record RemoveComboItemInput(Guid ComboId, Guid ItemId);
