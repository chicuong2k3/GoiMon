using FluentValidation;
using GoiMon.Api.Infrastructure.Data;
using GoiMon.Api.Features.Categories;

namespace GoiMon.Api.Features.Categories.Validators;

public class UpdateCategoryInputValidator : AbstractValidator<UpdateCategoryInput>
{
    public UpdateCategoryInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be at most 200 characters")
            .MustAsync(async (input, name, ct) =>
            {
                if (string.IsNullOrWhiteSpace(name)) return true;
                using var db = dbFactory.CreateDbContext();
                return !await db.Categories.AnyAsync(c => c.Id != input.Id && c.Name == name.Trim().ToLowerInvariant(), ct);
            })
            .WithMessage("Another category with the same name already exists");
    }
}
