using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Default Pilot Tenant
        var pilotTenant = await db.Tenants.FirstOrDefaultAsync(t => t.Slug == "goimon-lab");
        if (pilotTenant is null)
        {
            pilotTenant = new Domain.Entities.Tenant(Guid.Parse("00000000-0000-0000-0000-000000000001"), "GoiMon Lab", "goimon-lab");
            db.Tenants.Add(pilotTenant);
            await db.SaveChangesAsync();
        }

        var tenantId = pilotTenant.Id;

        // Categories
        if (!await db.Categories.AnyAsync())
        {
            var cat1 = new Domain.Entities.Category(Guid.NewGuid(), tenantId, "Noodles");
            var cat2 = new Domain.Entities.Category(Guid.NewGuid(), tenantId, "Sandwich");
            var cat3 = new Domain.Entities.Category(Guid.NewGuid(), tenantId, "Rice");
            db.Categories.AddRange(cat1, cat2, cat3);
            await db.SaveChangesAsync();
        }

        // Products
        if (!await db.Products.AnyAsync())
        {
            var categories = await db.Categories.OrderBy(c => c.Name).Take(3).ToListAsync();
            if (categories.Count >= 3)
            {
                db.Products.AddRange(new[] {
                    new Domain.Entities.Product(Guid.NewGuid(), tenantId, "Pho Bo", 40000m, categories[0].Id, "Beef noodle soup"),
                    new Domain.Entities.Product(Guid.NewGuid(), tenantId, "Banh Mi Thit", 25000m, categories[1].Id, "Vietnamese pork sandwich"),
                    new Domain.Entities.Product(Guid.NewGuid(), tenantId, "Com Ga", 45000m, categories[2].Id, "Chicken rice")
                });
                await db.SaveChangesAsync();
            }
        }

        // Product combos
        if (!await db.ProductCombos.AnyAsync())
        {
            var products = await db.Products.Take(2).ToListAsync();
            if (products.Count >= 2)
            {
                var combo = new Domain.Entities.ProductCombo(Guid.NewGuid(), tenantId, "Lunch Combo", products.Sum(p => p.Price) - 5000m);
                combo.AddItem(products[0].Id, 1);
                combo.AddItem(products[1].Id, 1);
                db.ProductCombos.Add(combo);
                await db.SaveChangesAsync();
            }
        }

        // Drinks
        var drinksCategory = await db.Categories.FirstOrDefaultAsync(c => c.Name == "Drinks");
        if (drinksCategory is null)
        {
            drinksCategory = new Domain.Entities.Category(Guid.NewGuid(), tenantId, "Drinks");
            db.Categories.Add(drinksCategory);
            await db.SaveChangesAsync();
        }

        var milkTea = await db.Products.FirstOrDefaultAsync(p => p.Name == "Milk Tea");
        if (milkTea is null)
        {
            milkTea = new Domain.Entities.Product(
                Guid.NewGuid(),
                tenantId,
                "Milk Tea",
                30000m,
                drinksCategory.Id,
                "Classic milk tea base");
            milkTea.UpdateUnitName("ly");
            db.Products.Add(milkTea);
            await db.SaveChangesAsync();
        }

        var hasVariants = await db.ProductVariants.AnyAsync(v => v.ProductId == milkTea.Id);
        if (!hasVariants)
        {
            db.ProductVariants.AddRange(
                new Domain.Entities.ProductVariant(Guid.NewGuid(), tenantId, milkTea.Id, "s", "Size S", 30000m, sortOrder: 1),
                new Domain.Entities.ProductVariant(Guid.NewGuid(), tenantId, milkTea.Id, "m", "Size M", 35000m, sortOrder: 2),
                new Domain.Entities.ProductVariant(Guid.NewGuid(), tenantId, milkTea.Id, "l", "Size L", 40000m, sortOrder: 3));
            await db.SaveChangesAsync();
        }

        var toppingGroup = await db.ModifierGroups.FirstOrDefaultAsync(g => g.ProductId == milkTea.Id && g.Name == "Topping");
        if (toppingGroup is null)
        {
            toppingGroup = new Domain.Entities.ModifierGroup(
                Guid.NewGuid(),
                tenantId,
                milkTea.Id,
                "Topping",
                Domain.Entities.ModifierSelectionMode.Multiple,
                minSelect: 0,
                maxSelect: 3,
                sortOrder: 1,
                isRequired: false,
                isActive: true);

            db.ModifierGroups.Add(toppingGroup);
            await db.SaveChangesAsync();
        }

        var hasToppingOptions = await db.ModifierOptions.AnyAsync(o => o.ModifierGroupId == toppingGroup.Id);
        if (!hasToppingOptions)
        {
            db.ModifierOptions.AddRange(
                new Domain.Entities.ModifierOption(Guid.NewGuid(), tenantId, toppingGroup.Id, "Pearl", 5000m, maxQty: 2, sortOrder: 1),
                new Domain.Entities.ModifierOption(Guid.NewGuid(), tenantId, toppingGroup.Id, "Pudding", 6000m, maxQty: 2, sortOrder: 2),
                new Domain.Entities.ModifierOption(Guid.NewGuid(), tenantId, toppingGroup.Id, "Grass Jelly", 5000m, maxQty: 2, sortOrder: 3));
            await db.SaveChangesAsync();
        }

        // Orders
        const int targetOrderCount = 50;
        var existingOrders = await db.Orders.CountAsync();
        if (existingOrders < targetOrderCount)
        {
            var allProducts = await db.Products.AsNoTracking().ToListAsync();
            var random = Random.Shared;
            var now = DateTimeOffset.UtcNow;

            for (var i = 0; i < (targetOrderCount - existingOrders); i++)
            {
                var order = new Domain.Entities.Order(Guid.NewGuid(), tenantId);
                var itemCount = random.Next(1, 4);
                var selected = allProducts.OrderBy(_ => random.Next()).Take(itemCount).ToList();

                foreach (var p in selected)
                {
                    order.AddItem(p.Id, p.Name, random.Next(1, 3), p.Price, p.UnitName);
                }

                if (random.Next(100) < 30) order.MarkCompleted();
                
                var createdAt = now.AddDays(-random.Next(0, 14)).AddHours(-random.Next(0, 24));
                db.Entry(order).Property(o => o.CreatedAt).CurrentValue = createdAt;
                db.Orders.Add(order);
            }
            await db.SaveChangesAsync();
        }
    }
}
