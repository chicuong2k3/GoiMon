using FluentValidation;

namespace GoiMon.Api.Features.Orders.Validators;

public class OrderInputValidator : AbstractValidator<OrderInput>
{
    public OrderInputValidator()
    {
        RuleFor(x => x.Items).NotNull().NotEmpty().WithMessage("Order must contain at least one item");
        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductName)
                .NotEmpty().WithMessage("Product name snapshot is required")
                .MaximumLength(255);
            items.RuleFor(i => i.UnitName)
                .MaximumLength(50).When(i => i.UnitName is not null);
            items.RuleFor(i => i.Qty).GreaterThan(0);
            items.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
