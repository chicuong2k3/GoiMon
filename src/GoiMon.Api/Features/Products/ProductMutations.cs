using GoiMon.Api.Features.ImageUpload.Services;

namespace GoiMon.Api.Features.Products;

[ExtendObjectType("Mutation")]
public class ProductMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product> CreateProduct(
        ProductInput input,
        [GraphQLType(typeof(UploadType))] IFile? image,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageUpload)
    {
        var p = new Product(Guid.NewGuid(), input.Name, input.Price, input.CategoryId, input.Description);

        if (image is not null)
        {
            await using var stream = image.OpenReadStream();
            var url = await imageUpload.UploadImageAsync(stream, image.Name, "products");
            p.UpdateImage(url);
        }
        else if (!string.IsNullOrEmpty(input.ImageUrl))
        {
            p.UpdateImage(input.ImageUrl);
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
        [GraphQLType(typeof(UploadType))] IFile? image,
        [Service(ServiceKind.Pooled)] AppDbContext db,
        [Service] IImageUploadService imageUpload)
    {
        var p = await db.Products.FindAsync(input.Id);
        if (p is null) return null;

        p.Rename(input.Name);
        p.ChangePrice(input.Price);
        p.ChangeCategory(input.CategoryId);
        p.UpdateDescription(input.Description);

        if (image is not null)
        {
            await using var stream = image.OpenReadStream();
            var url = await imageUpload.UploadImageAsync(stream, image.Name, "products");
            p.UpdateImage(url);
        }
        else if (input.ImageUrl is not null)
        {
            p.UpdateImage(string.IsNullOrEmpty(input.ImageUrl) ? null : input.ImageUrl);
        }

        // Old image cleanup is handled by ImageCleanupHandler via domain event
        await db.SaveChangesAsync();
        return p;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteProduct(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return false;
        p.MarkDeleted(); // Raises ProductDeletedEvent → ImageCleanupHandler deletes image
        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return true;
    }
}

public record ProductInput(string Name, decimal Price, Guid? CategoryId, string? Description, string? ImageUrl = null);
public record UpdateProductInput(Guid Id, string Name, decimal Price, Guid? CategoryId, string? Description, string? ImageUrl = null);
