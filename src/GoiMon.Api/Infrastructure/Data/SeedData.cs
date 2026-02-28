using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Products
        if (!await db.Categories.AnyAsync())
        {
            var cat1 = new GoiMon.Api.Domain.Entities.Category(Guid.NewGuid(), "Noodles");
            var cat2 = new GoiMon.Api.Domain.Entities.Category(Guid.NewGuid(), "Sandwich");
            var cat3 = new GoiMon.Api.Domain.Entities.Category(Guid.NewGuid(), "Rice");
            db.Categories.AddRange(cat1, cat2, cat3);
            await db.SaveChangesAsync();

            if (!await db.Products.AnyAsync())
            {
                db.Products.AddRange(new[] {
                    new GoiMon.Api.Domain.Entities.Product(Guid.NewGuid(), "Pho Bo", 40000, cat1.Id),
                    new GoiMon.Api.Domain.Entities.Product(Guid.NewGuid(), "Banh Mi Thit", 25000, cat2.Id),
                    new GoiMon.Api.Domain.Entities.Product(Guid.NewGuid(), "Com Ga", 45000, cat3.Id)
                });
                await db.SaveChangesAsync();
            }
        }

        // Product combos (seed a sample combo if none exist)
        if (!await db.ProductCombos.AnyAsync())
        {
            var products = await db.Products.Take(2).ToListAsync();
            if (products.Count >= 2)
            {
                var combo = new GoiMon.Api.Domain.Entities.ProductCombo(Guid.NewGuid(), "Lunch Combo", products.Sum(p => p.PriceCents) - 5000);
                combo.AddItem(products[0].Id, 1);
                combo.AddItem(products[1].Id, 1);
                db.ProductCombos.Add(combo);
                await db.SaveChangesAsync();
            }
        }
    }
}
