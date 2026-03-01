using GoiMon.Api.Features.ImageUpload.Services;

namespace GoiMon.Api.Features.ImageUpload.Mutations;

[MutationType]
public class ImageUploadMutations
{
    /// <summary>
    /// Upload an image for a product.
    /// </summary>
    public async Task<ProductImageUploadPayload> UploadProductImage(
        Guid productId,
        IFile file,
        [Service] IImageUploadService imageService,
        [Service] AppDbContext dbContext,
        CancellationToken ct)
    {
        var product = await dbContext.Products.FindAsync(new object[] { productId }, ct);
        if (product is null)
            return new ProductImageUploadPayload("Product not found", null);

        await using var stream = file.OpenReadStream();
        var imageUrl = await imageService.UploadImageAsync(stream, file.Name, "products");

        // Delete old image if exists
        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            try
            {
                await imageService.DeleteImageAsync(product.ImageUrl);
            }
            catch (Exception ex)
            {
                // Log but don't fail the upload
                Console.WriteLine($"Failed to delete old image: {ex.Message}");
            }
        }

        product.UpdateImage(imageUrl);
        await dbContext.SaveChangesAsync(ct);

        return new ProductImageUploadPayload(null, new ProductImageUploadResult(productId, imageUrl));
    }

    /// <summary>
    /// Upload an image for a combo.
    /// </summary>
    public async Task<ComboImageUploadPayload> UploadComboImage(
        Guid comboId,
        IFile file,
        [Service] IImageUploadService imageService,
        [Service] AppDbContext dbContext,
        CancellationToken ct)
    {
        var combo = await dbContext.ProductCombos.FindAsync(new object[] { comboId }, ct);
        if (combo is null)
            return new ComboImageUploadPayload("Combo not found", null);

        await using var stream = file.OpenReadStream();
        var imageUrl = await imageService.UploadImageAsync(stream, file.Name, "combos");

        // Delete old image if exists
        if (!string.IsNullOrEmpty(combo.ImageUrl))
        {
            try
            {
                await imageService.DeleteImageAsync(combo.ImageUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to delete old combo image: {ex.Message}");
            }
        }

        combo.UpdateImage(imageUrl);
        await dbContext.SaveChangesAsync(ct);

        return new ComboImageUploadPayload(null, new ComboImageUploadResult(comboId, imageUrl));
    }

    /// <summary>
    /// Delete a product image.
    /// </summary>
    public async Task<DeleteImagePayload> DeleteProductImage(
        Guid productId,
        [Service] IImageUploadService imageService,
        [Service] AppDbContext dbContext,
        CancellationToken ct)
    {
        var product = await dbContext.Products.FindAsync(new object[] { productId }, ct);
        if (product is null)
            return new DeleteImagePayload("Product not found", false);

        if (string.IsNullOrEmpty(product.ImageUrl))
            return new DeleteImagePayload("No image to delete", false);

        await imageService.DeleteImageAsync(product.ImageUrl);
        product.UpdateImage(null);
        await dbContext.SaveChangesAsync(ct);

        return new DeleteImagePayload(null, true);
    }

    /// <summary>
    /// Delete a combo image.
    /// </summary>
    public async Task<DeleteImagePayload> DeleteComboImage(
        Guid comboId,
        [Service] IImageUploadService imageService,
        [Service] AppDbContext dbContext,
        CancellationToken ct)
    {
        var combo = await dbContext.ProductCombos.FindAsync(new object[] { comboId }, ct);
        if (combo is null)
            return new DeleteImagePayload("Combo not found", false);

        if (string.IsNullOrEmpty(combo.ImageUrl))
            return new DeleteImagePayload("No image to delete", false);

        await imageService.DeleteImageAsync(combo.ImageUrl);
        combo.UpdateImage(null);
        await dbContext.SaveChangesAsync(ct);

        return new DeleteImagePayload(null, true);
    }
}

public record ProductImageUploadPayload(string? Error, ProductImageUploadResult? Result);
public record ProductImageUploadResult(Guid ProductId, string ImageUrl);

public record ComboImageUploadPayload(string? Error, ComboImageUploadResult? Result);
public record ComboImageUploadResult(Guid ComboId, string ImageUrl);

public record DeleteImagePayload(string? Error, bool Success);
