using FluentValidation;

namespace GoiMon.Api.Features.Orders.Validators;

public class CreateOrderInputValidator : AbstractValidator<CreateOrderInput>
{
    public CreateOrderInputValidator()
    {
        RuleFor(x => x.Lines)
            .NotNull()
            .NotEmpty()
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

            line.RuleForEach(x => x.Modifiers ?? new List<CreateOrderLineModifierInput>()).ChildRules(modifier =>
            {
                modifier.RuleFor(x => x.OptionId)
                    .NotEqual(Guid.Empty)
                    .WithMessage("OptionId is required");

                modifier.RuleFor(x => x.Quantity)
                    .GreaterThan(0)
                    .WithMessage("Modifier quantity must be greater than zero");
            });
        });
    }
}
