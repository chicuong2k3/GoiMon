using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Products
        if (!await db.Products.AnyAsync())
        {
            db.Products.AddRange(new[] {
                new GoiMon.Api.Domain.Entities.Product(Guid.NewGuid(), "Pho Bo", 40000, "Noodles"),
                new GoiMon.Api.Domain.Entities.Product(Guid.NewGuid(), "Banh Mi Thit", 25000, "Sandwich"),
                new GoiMon.Api.Domain.Entities.Product(Guid.NewGuid(), "Com Ga", 45000, "Rice")
            });
            await db.SaveChangesAsync();
        }
    }
}
