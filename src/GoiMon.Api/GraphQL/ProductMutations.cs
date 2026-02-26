using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType("Mutation")]
public class ProductMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<Product> AddProduct(ProductInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var p = new Product(Guid.NewGuid(), input.Name, input.PriceCents, input.Category);
        db.Products.Add(p);
        await db.SaveChangesAsync();
        return p;
    }
}

public record ProductInput(string Name, int PriceCents, string Category);
