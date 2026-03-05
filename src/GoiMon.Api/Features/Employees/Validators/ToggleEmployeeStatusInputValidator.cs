using FluentValidation;
using GoiMon.Api.Features.Employees.Models;

namespace GoiMon.Api.Features.Employees.Validators;

public sealed class ToggleEmployeeStatusInputValidator : AbstractValidator<ToggleEmployeeStatusInput>
{
    public ToggleEmployeeStatusInputValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
