using FluentValidation;

namespace GoiMon.Api.Features.Orders.Validators;

public class CreateOrderInputValidator : AbstractValidator<CreateOrderInput>
{
    public CreateOrderInputValidator()
    {
        RuleFor(x => x.Lines)
            .NotNull()
            .WithMessage("Lines cannot be null");

        RuleFor(x => x)
            .Must(input => (input.Lines?.Count ?? 0) + (input.ComboLines?.Count ?? 0) > 0)
            .WithMessage("Order must contain at least one line");

        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(x => x.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage("ProductId is required");

            line.RuleFor(x => x.VariantId)
                .Must(variantId => variantId is null || variantId.Value != Guid.Empty)
                .WithMessage("VariantId, when provided, must be a valid id");

            line.RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero");

            line.RuleFor(x => x.Modifiers)
                .Must(modifiers => modifiers is null || modifiers.Select(m => m.OptionId).Distinct().Count() == modifiers.Count)
                .WithMessage("Modifier options cannot contain duplicates");

            line.When(x => x.Modifiers is not null, () =>
            {
                line.RuleForEach(x => x.Modifiers!).ChildRules(modifier =>
                {
                    modifier.RuleFor(x => x.OptionId)
                        .NotEqual(Guid.Empty)
                        .WithMessage("OptionId is required");

                    modifier.RuleFor(x => x.Quantity)
                        .GreaterThan(0)
                        .WithMessage("Modifier quantity must be greater than zero");
                });
            });
        });

        When(x => x.ComboLines is not null, () =>
        {
            RuleForEach(x => x.ComboLines!).ChildRules(line =>
            {
                line.RuleFor(x => x.ComboId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("ComboId is required");

                line.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Quantity must be greater than zero");
            });
        });
    }
}
