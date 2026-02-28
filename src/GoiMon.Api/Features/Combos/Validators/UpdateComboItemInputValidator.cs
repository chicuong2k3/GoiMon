using FluentValidation;

namespace GoiMon.Api.Features.Combos.Validators;

public class UpdateComboItemInputValidator : AbstractValidator<UpdateComboItemInput>
{
    public UpdateComboItemInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.ComboId)
            .MustAsync(async (id, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return await db.ProductCombos.AnyAsync(c => c.Id == id, ct);
            })
            .WithMessage("Combo does not exist");

        RuleFor(x => x.ItemId)
            .MustAsync(async (input, itemId, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return await db.ProductComboItems.AnyAsync(i => i.Id == itemId && i.ComboId == input.ComboId, ct);
            })
            .WithMessage("Item does not exist in this combo");

        RuleFor(x => x.Qty)
            .GreaterThan(0).WithMessage("Qty must be greater than 0");
    }
}
