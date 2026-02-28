using GoiMon.Api.Data;
using GoiMon.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoiMon.Api.GraphQL;

[ExtendObjectType("Mutation")]
public class CategoryMutations
{
    [UseDbContext(typeof(AppDbContext))]
    public async Task<Category> CreateCategory(CreateCategoryInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var c = new Category(Guid.NewGuid(), input.Name);
        db.Categories.Add(c);
        await db.SaveChangesAsync();
        return c;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<Category?> UpdateCategory(UpdateCategoryInput input, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var c = await db.Categories.FirstOrDefaultAsync(x => x.Id == input.Id);
        if (c is null) return null;
        if (input.Name is not null)
            c = new Category(c.Id, input.Name); // simple replace to preserve immutability-like pattern
        db.Categories.Update(c);
        await db.SaveChangesAsync();
        return c;
    }

    [UseDbContext(typeof(AppDbContext))]
    public async Task<bool> DeleteCategory(Guid id, [Service(ServiceKind.Pooled)] AppDbContext db)
    {
        var c = await db.Categories.FirstOrDefaultAsync(x => x.Id == id);
        if (c is null) return false;
        db.Categories.Remove(c);
        await db.SaveChangesAsync();
        return true;
    }
}

public record CreateCategoryInput(string Name);
public record UpdateCategoryInput(Guid Id, string? Name);
