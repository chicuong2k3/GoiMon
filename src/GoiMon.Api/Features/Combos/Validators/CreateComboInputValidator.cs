using FluentValidation;
using GoiMon.Api.Features.Combos;
using GoiMon.Api.Infrastructure.Data;

namespace GoiMon.Api.Features.Combos.Validators;

public class CreateComboInputValidator : AbstractValidator<CreateComboInput>
{
    public CreateComboInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters")
            .MustAsync(async (name, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return !await db.ProductCombos.AnyAsync(c => c.Name == name.Trim().ToLowerInvariant(), ct);
            })
            .WithMessage("A combo with the same name already exists");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative");

        RuleForEach(x => x.Items)
            .ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId).NotEmpty().WithMessage("ProductId is required");
                item.RuleFor(i => i.Qty).GreaterThan(0).WithMessage("Qty must be greater than 0");
            })
            .When(x => x.Items is not null);

        RuleFor(x => x.Items)
            .MustAsync(async (items, ct) =>
            {
                var productIds = items!.Select(i => i.ProductId).Distinct().ToList();
                using var db = dbFactory.CreateDbContext();
                var found = await db.Products.CountAsync(p => productIds.Contains(p.Id), ct);
                return found == productIds.Count;
            })
            .WithMessage("One or more product IDs do not exist")
            .When(x => x.Items is not null && x.Items.Count > 0);
    }
}
