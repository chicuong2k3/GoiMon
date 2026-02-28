using FluentValidation;

namespace GoiMon.Api.GraphQL.Validators;

public class ProductInputValidator : AbstractValidator<ProductInput>
{
    public ProductInputValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Product name is required");
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price must be >= 0");
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
    }
}
