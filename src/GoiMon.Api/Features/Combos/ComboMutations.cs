using GoiMon.Api.Features.ImageUpload.Services;

namespace GoiMon.Api.Features.Combos;

[ExtendObjectType("Mutation")]
public class ComboMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo> CreateCombo(
        CreateComboInput input, 
        IFile? image,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageService)
    {
        var combo = new ProductCombo(Guid.NewGuid(), input.Name, input.Price);
        if (input.Items is not null)
        {
            foreach (var item in input.Items)
                combo.AddItem(item.ProductId, item.Qty);
        }
        
        // Upload image if provided
        if (image is not null)
        {
            await using var stream = image.OpenReadStream();
            var imageUrl = await imageService.UploadImageAsync(stream, image.Name, "combos");
            combo.UpdateImage(imageUrl);
        }
        
        db.ProductCombos.Add(combo);
        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> UpdateCombo(
        UpdateComboInput input, 
        IFile? image,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageService)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == input.Id);
        if (combo is null) return null;

        if (input.Name is not null)
            combo.UpdateName(input.Name);
        if (input.Price.HasValue)
            combo.UpdatePrice(input.Price.Value);

        // Handle image upload/update
        if (image is not null)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(combo.ImageUrl))
            {
                try
                {
                    await imageService.DeleteImageAsync(combo.ImageUrl);
                }
                catch
                {
                    // Log but don't fail the update
                }
            }
            
            // Upload new image
            await using var stream = image.OpenReadStream();
            var imageUrl = await imageService.UploadImageAsync(stream, image.Name, "combos");
            combo.UpdateImage(imageUrl);
        }

        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteCombo(
        Guid id, 
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageService)
    {
        var combo = await db.ProductCombos.FindAsync(id);
        if (combo is null) return false;
        
        // Delete image from Cloudinary if exists
        if (!string.IsNullOrEmpty(combo.ImageUrl))
        {
            try
            {
                await imageService.DeleteImageAsync(combo.ImageUrl);
            }
            catch
            {
                // Log but don't fail the delete
            }
        }
        
        db.ProductCombos.Remove(combo);
        await db.SaveChangesAsync();
        return true;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> AddComboItem(AddComboItemInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == input.ComboId);
        if (combo is null) return null;
        combo.AddItem(input.ProductId, input.Qty);
        // Explicitly track the new entity — EF Core does not reliably detect new items
        // added to navigation collections when ValueGeneratedOnAdd is set on the key.
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
}

public record CreateComboInput(string Name, decimal Price, List<ComboItemInput>? Items);
public record UpdateComboInput(Guid Id, string? Name, decimal? Price);
public record ComboItemInput(Guid ProductId, int Qty);
public record AddComboItemInput(Guid ComboId, Guid ProductId, int Qty);
public record UpdateComboItemInput(Guid ComboId, Guid ItemId, int Qty);
public record RemoveComboItemInput(Guid ComboId, Guid ItemId);
