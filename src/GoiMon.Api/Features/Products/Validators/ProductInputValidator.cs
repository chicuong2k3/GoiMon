using FluentValidation;
using GoiMon.Api.Features.Products;

namespace GoiMon.Api.Features.Products.Validators;

public class ProductInputValidator : AbstractValidator<ProductInput>
{
    public ProductInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MustAsync(async (name, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return !await db.Products.AnyAsync(p => p.Name == name.Trim().ToLowerInvariant(), ct);
            })
            .WithMessage("A product with the same name already exists");

        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);

        RuleFor(x => x.CategoryId)
            .MustAsync(async (id, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return await db.Categories.AnyAsync(c => c.Id == id!.Value, ct);
            })
            .WithMessage("Category does not exist")
            .When(x => x.CategoryId.HasValue);
    }
}
