using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using GoiMon.Api.GraphQL.Types;
using System.Collections.Generic;

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

    [UseDbContext(typeof(AppDbContext))]
    public async Task<List<Product>> AddProducts(List<ProductInput> inputs, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var created = new List<Product>();
        foreach (var i in inputs)
        {
            var p = new Product(Guid.NewGuid(), i.Name, i.PriceCents, i.Category);
            db.Products.Add(p);
            created.Add(p);
        }
        await db.SaveChangesAsync();
        return created;
    }
}

public record ProductInput(string Name, int PriceCents, string Category);
