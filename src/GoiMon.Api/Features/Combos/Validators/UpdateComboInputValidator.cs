using FluentValidation;

namespace GoiMon.Api.Features.Combos.Validators;

public class UpdateComboInputValidator : AbstractValidator<UpdateComboInput>
{
    public UpdateComboInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name cannot be empty")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters")
            .MustAsync(async (input, name, ct) =>
            {
                if (string.IsNullOrWhiteSpace(name)) return true;
                using var db = dbFactory.CreateDbContext();
                var trimmedName = name.Trim();
                return !await db.ProductCombos.AnyAsync(c => c.Id != input.Id && c.Name == trimmedName, ct);
            })
            .WithMessage("Another combo with the same name already exists")
            .When(x => x.Name is not null);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price must be non-negative")
            .When(x => x.Price.HasValue);
    }
}
