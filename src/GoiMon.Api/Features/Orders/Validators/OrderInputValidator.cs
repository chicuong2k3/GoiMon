using FluentValidation;

namespace GoiMon.Api.GraphQL.Validators;

public class OrderInputValidator : AbstractValidator<OrderInput>
{
    public OrderInputValidator()
    {
        RuleFor(x => x.Items).NotNull().NotEmpty().WithMessage("Order must contain at least one item");
        RuleForEach(x => x.Items).ChildRules(items =>
        {
            items.RuleFor(i => i.ProductId).NotEmpty();
            items.RuleFor(i => i.Qty).GreaterThan(0);
            items.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
        });
    }
}
