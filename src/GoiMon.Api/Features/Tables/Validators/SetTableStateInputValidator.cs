using FluentValidation;
using GoiMon.Api.Features.Tables.Models;

namespace GoiMon.Api.Features.Tables.Validators;

public sealed class SetTableStateInputValidator : AbstractValidator<SetTableStateInput>
{
    public SetTableStateInputValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.State).IsInEnum();
    }
}
