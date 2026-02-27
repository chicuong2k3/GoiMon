using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType("Mutation")]
public class ComboMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo> CreateCombo(CreateComboInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = new ProductCombo(Guid.NewGuid(), input.Name, input.PriceCents);
        foreach (var it in input.Items)
        {
            combo.AddItem(it.ProductId, it.Qty);
        }

        db.ProductCombos.Add(combo);
        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> UpdateCombo(UpdateComboInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == input.Id);
        if (combo is null) return null;

        if (input.Name is not null)
            combo.UpdateName(input.Name);

        if (input.PriceCents is not null)
            combo.UpdatePrice(input.PriceCents.Value);

        // Replace items if provided
        if (input.Items is not null)
        {
            combo.ClearItems();
            foreach (var it in input.Items)
            {
                combo.AddItem(it.ProductId, it.Qty);
            }
        }

        db.ProductCombos.Update(combo);
        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteCombo(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == id);
        if (combo is null) return false;
        db.ProductCombos.Remove(combo);
        await db.SaveChangesAsync();
        return true;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> AddComboItem(Guid comboId, CreateComboItemInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == comboId);
        if (combo is null) return null;

        combo.AddItem(input.ProductId, input.Qty);
        db.ProductCombos.Update(combo);
        await db.SaveChangesAsync();
        return combo;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<ProductCombo?> RemoveComboItem(Guid comboId, Guid itemId, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var combo = await db.ProductCombos.Include(c => c.Items).FirstOrDefaultAsync(c => c.Id == comboId);
        if (combo is null) return null;

        combo.RemoveItem(itemId);
        db.ProductCombos.Update(combo);
        await db.SaveChangesAsync();
        return combo;
    }
}

public record UpdateComboInput(Guid Id, string? Name, int? PriceCents, List<CreateComboItemInput>? Items);

public record CreateComboInput(string Name, int PriceCents, List<CreateComboItemInput> Items);
public record CreateComboItemInput(Guid ProductId, int Qty);
