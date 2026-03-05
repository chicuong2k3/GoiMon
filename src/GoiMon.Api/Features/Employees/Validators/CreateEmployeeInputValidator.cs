using FluentValidation;
using GoiMon.Api.Features.Employees.Models;

namespace GoiMon.Api.Features.Employees.Validators;

public sealed class CreateEmployeeInputValidator : AbstractValidator<CreateEmployeeInput>
{
    public CreateEmployeeInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email format is invalid")
            .MustAsync(async (email, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                var normalized = email.Trim().ToLowerInvariant();
                return !await db.Users.AnyAsync(u => u.Email == normalized, ct);
            })
            .WithMessage("An employee with the same email already exists");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .When(x => x.Phone is not null);

        RuleFor(x => x.FirstName)
            .MaximumLength(100)
            .When(x => x.FirstName is not null);

        RuleFor(x => x.LastName)
            .MaximumLength(100)
            .When(x => x.LastName is not null);

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role is invalid");
    }
}
