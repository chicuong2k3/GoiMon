namespace GoiMon.Api.Features.Products;

[ExtendObjectType("Mutation")]
public class ProductMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product> CreateProduct(ProductInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var p = new Product(Guid.NewGuid(), input.Name, input.Price, input.CategoryId, input.Description);
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
    public async Task<Product?> UpdateProduct(UpdateProductInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var p = await db.Products.FindAsync(input.Id);
        if (p is null) return null;

        p.Rename(input.Name);
        p.ChangePrice(input.Price);
        p.ChangeCategory(input.CategoryId);
        p.UpdateDescription(input.Description);

        await db.SaveChangesAsync();
        return p;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteProduct(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return false;

        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return true;
    }
}

public record ProductInput(string Name, decimal Price, Guid? CategoryId, string? Description);
public record UpdateProductInput(Guid Id, string Name, decimal Price, Guid? CategoryId, string? Description);
