using FluentValidation;
using GoiMon.Api.Features.Products;
using GoiMon.Api.Infrastructure.Data;

namespace GoiMon.Api.Features.Products.Validators;

public class UpdateProductInputValidator : AbstractValidator<UpdateProductInput>
{
    public UpdateProductInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MustAsync(async (input, name, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                return !await db.Products.AnyAsync(p => p.Id != input.Id && p.Name == name.Trim().ToLowerInvariant(), ct);
            })
            .WithMessage("Another product with the same name already exists");

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
