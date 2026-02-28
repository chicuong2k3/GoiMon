using FluentValidation;

namespace GoiMon.Api.Features.Combos.Validators;

public class AddComboItemInputValidator : AbstractValidator<AddComboItemInput>
{
    public AddComboItemInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.ComboId)
            .MustAsync(async (id, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return await db.ProductCombos.AnyAsync(c => c.Id == id, ct);
            })
            .WithMessage("Combo does not exist");

        RuleFor(x => x.ProductId)
            .Cascade(CascadeMode.Stop)
            .MustAsync(async (id, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return await db.Products.AnyAsync(p => p.Id == id, ct);
            })
            .WithMessage("Product does not exist")
            .MustAsync(async (input, productId, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return !await db.ProductComboItems.AnyAsync(i => i.ComboId == input.ComboId && i.ProductId == productId, ct);
            })
            .WithMessage("Product is already in this combo");

        RuleFor(x => x.Qty)
            .GreaterThan(0).WithMessage("Qty must be greater than 0");
    }
}
