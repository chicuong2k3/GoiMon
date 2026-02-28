using FluentValidation;

namespace GoiMon.Api.Features.Products.Validators;

public class ProductInputListValidator : AbstractValidator<List<ProductInput>>
{
    public ProductInputListValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        // Each item must pass the same per-item rules
        RuleForEach(x => x).SetValidator(new ProductInputItemValidator());

        // No duplicate names within the submitted list
        RuleFor(x => x)
            .Must(inputs =>
            {
                var normalized = inputs.Select(i => i.Name.Trim().ToLowerInvariant()).ToList();
                return normalized.Count == normalized.Distinct().Count();
            })
            .WithMessage("Duplicate product names in the input list");

        // No name in the list already exists in the DB (single batch query)
        RuleFor(x => x)
            .MustAsync(async (inputs, ct) =>
            {
                var normalized = inputs.Select(i => i.Name.Trim().ToLowerInvariant()).ToList();
                using var db = dbFactory.CreateDbContext();
                return !await db.Products.AnyAsync(p => normalized.Contains(p.Name), ct);
            })
            .WithMessage("One or more product names already exist in the database");

        // All referenced categories must exist (single batch query)
        RuleFor(x => x)
            .MustAsync(async (inputs, ct) =>
            {
                var categoryIds = inputs
                    .Where(i => i.CategoryId.HasValue)
                    .Select(i => i.CategoryId!.Value)
                    .Distinct()
                    .ToList();
                if (categoryIds.Count == 0) return true;
                using var db = dbFactory.CreateDbContext();
                var found = await db.Categories.CountAsync(c => categoryIds.Contains(c.Id), ct);
                return found == categoryIds.Count;
            })
            .WithMessage("One or more category IDs do not exist");
    }
}

// Validates a single ProductInput without the DB uniqueness check (used inside the list validator)
file sealed class ProductInputItemValidator : AbstractValidator<ProductInput>
{
    public ProductInputItemValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
    }
}
