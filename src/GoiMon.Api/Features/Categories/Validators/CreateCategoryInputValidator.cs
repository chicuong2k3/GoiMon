using FluentValidation;

namespace GoiMon.Api.Features.Categories.Validators;

public class CreateCategoryInputValidator : AbstractValidator<CreateCategoryInput>
{
    public CreateCategoryInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters")
            .MustAsync(async (name, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                var trimmedName = name.Trim();
                return !await db.Categories.AnyAsync(c => c.Name == trimmedName, ct);
            })
            .WithMessage("A category with the same name already exists");
    }
}
