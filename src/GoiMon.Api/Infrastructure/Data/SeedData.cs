using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        // Categories
        if (!await db.Categories.AnyAsync())
        {
            var cat1 = new Domain.Entities.Category(Guid.NewGuid(), "Noodles");
            var cat2 = new Domain.Entities.Category(Guid.NewGuid(), "Sandwich");
            var cat3 = new Domain.Entities.Category(Guid.NewGuid(), "Rice");
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
                    new Domain.Entities.Product(Guid.NewGuid(), "Pho Bo", 40000m, categories[0].Id, "Beef noodle soup"),
                    new Domain.Entities.Product(Guid.NewGuid(), "Banh Mi Thit", 25000m, categories[1].Id, "Vietnamese pork sandwich"),
                    new Domain.Entities.Product(Guid.NewGuid(), "Com Ga", 45000m, categories[2].Id, "Chicken rice")
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
                var combo = new Domain.Entities.ProductCombo(Guid.NewGuid(), "Lunch Combo", products.Sum(p => p.Price) - 5000m);
                combo.AddItem(products[0].Id, 1);
                combo.AddItem(products[1].Id, 1);
                db.ProductCombos.Add(combo);
                await db.SaveChangesAsync();
            }
        }

        // Configurable beverage sample (size + topping)
        // Keep this seed idempotent so reruns don't create duplicates.
        var drinksCategory = await db.Categories.FirstOrDefaultAsync(c => c.Name == "drinks");
        if (drinksCategory is null)
        {
            drinksCategory = new Domain.Entities.Category(Guid.NewGuid(), "Drinks");
            db.Categories.Add(drinksCategory);
            await db.SaveChangesAsync();
        }

        var milkTea = await db.Products.FirstOrDefaultAsync(p => p.Name == "milk tea");
        if (milkTea is null)
        {
            milkTea = new Domain.Entities.Product(
                Guid.NewGuid(),
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
                new Domain.Entities.ProductVariant(Guid.NewGuid(), milkTea.Id, "s", "Size S", 30000m, sortOrder: 1),
                new Domain.Entities.ProductVariant(Guid.NewGuid(), milkTea.Id, "m", "Size M", 35000m, sortOrder: 2),
                new Domain.Entities.ProductVariant(Guid.NewGuid(), milkTea.Id, "l", "Size L", 40000m, sortOrder: 3));
            await db.SaveChangesAsync();
        }

        var toppingGroup = await db.ModifierGroups.FirstOrDefaultAsync(g => g.ProductId == milkTea.Id && g.Name == "Topping");
        if (toppingGroup is null)
        {
            toppingGroup = new Domain.Entities.ModifierGroup(
                Guid.NewGuid(),
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
                new Domain.Entities.ModifierOption(Guid.NewGuid(), toppingGroup.Id, "Pearl", 5000m, maxQty: 2, sortOrder: 1),
                new Domain.Entities.ModifierOption(Guid.NewGuid(), toppingGroup.Id, "Pudding", 6000m, maxQty: 2, sortOrder: 2),
                new Domain.Entities.ModifierOption(Guid.NewGuid(), toppingGroup.Id, "Grass Jelly", 5000m, maxQty: 2, sortOrder: 3));
            await db.SaveChangesAsync();
        }

        // Orders - seed many records for dashboard/testing convenience
        const int targetOrderCount = 300;
        var existingOrders = await db.Orders.CountAsync();
        if (existingOrders >= targetOrderCount)
            return;

        var allProducts = await db.Products.AsNoTracking().ToListAsync();
        if (allProducts.Count == 0)
            return;

        var random = Random.Shared;
        var toCreate = targetOrderCount - existingOrders;
        var now = DateTimeOffset.UtcNow;

        for (var i = 0; i < toCreate; i++)
        {
            var order = new Domain.Entities.Order(Guid.NewGuid());

            var itemCount = random.Next(1, Math.Min(5, allProducts.Count) + 1);
            var selectedProducts = allProducts
                .OrderBy(_ => random.Next())
                .Take(itemCount)
                .ToList();

            foreach (var product in selectedProducts)
            {
                var qty = random.Next(1, 4);
                order.AddItem(
                    product.Id,
                    product.Name,
                    qty,
                    product.Price,
                    product.UnitName);
            }

            // Status distribution: mostly Open, some Completed, some Cancelled
            var roll = random.Next(100);
            if (roll < 25)
            {
                order.MarkCompleted();
            }
            else if (roll < 35)
            {
                order.Cancel();
            }

            // Spread created time across recent 14 days for realistic testing
            var createdAt = now
                .AddDays(-random.Next(0, 14))
                .AddHours(-random.Next(0, 24))
                .AddMinutes(-random.Next(0, 60));
            db.Entry(order).Property(o => o.CreatedAt).CurrentValue = createdAt;

            db.Orders.Add(order);
        }

        await db.SaveChangesAsync();
    }
}
