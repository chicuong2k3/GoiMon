using GoiMon.Api.Features.ImageUpload.Services;

namespace GoiMon.Api.Features.Products;

[ExtendObjectType("Mutation")]
public class ProductMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product> CreateProduct(
        ProductInput input, 
        IFile? image,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageService)
    {
        var p = new Product(Guid.NewGuid(), input.Name, input.Price, input.CategoryId, input.Description);
        
        // Upload image if provided
        if (image is not null)
        {
            await using var stream = image.OpenReadStream();
            var imageUrl = await imageService.UploadImageAsync(stream, image.Name, "products");
            p.UpdateImage(imageUrl);
        }
        
        db.Products.Add(p);
        await db.SaveChangesAsync();
        return p;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<List<Product>> CreateProducts(List<ProductInput> inputs, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var created = new List<Product>();
        foreach (var i in inputs)
        {
            var p = new Product(Guid.NewGuid(), i.Name, i.Price, i.CategoryId, i.Description);
            db.Products.Add(p);
            created.Add(p);
        }
        await db.SaveChangesAsync();
        return created;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product?> UpdateProduct(
        UpdateProductInput input, 
        IFile? image,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageService)
    {
        var p = await db.Products.FindAsync(input.Id);
        if (p is null) return null;

        p.Rename(input.Name);
        p.ChangePrice(input.Price);
        p.ChangeCategory(input.CategoryId);
        p.UpdateDescription(input.Description);

        // Handle image upload/update
        if (image is not null)
        {
            // Delete old image if exists
            if (!string.IsNullOrEmpty(p.ImageUrl))
            {
                try
                {
                    await imageService.DeleteImageAsync(p.ImageUrl);
                }
                catch
                {
                    // Log but don't fail the update
                }
            }
            
            // Upload new image
            await using var stream = image.OpenReadStream();
            var imageUrl = await imageService.UploadImageAsync(stream, image.Name, "products");
            p.UpdateImage(imageUrl);
        }

        await db.SaveChangesAsync();
        return p;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteProduct(
        Guid id, 
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageService)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return false;

        // Delete image from Cloudinary if exists
        if (!string.IsNullOrEmpty(p.ImageUrl))
        {
            try
            {
                await imageService.DeleteImageAsync(p.ImageUrl);
            }
            catch
            {
                // Log but don't fail the delete
            }
        }

        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return true;
    }
}

public record ProductInput(string Name, decimal Price, Guid? CategoryId, string? Description);
public record UpdateProductInput(Guid Id, string Name, decimal Price, Guid? CategoryId, string? Description);
