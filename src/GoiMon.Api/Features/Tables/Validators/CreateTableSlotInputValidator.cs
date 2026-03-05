using FluentValidation;
using GoiMon.Api.Features.Tables.Models;

namespace GoiMon.Api.Features.Tables.Validators;

public sealed class CreateTableSlotInputValidator : AbstractValidator<CreateTableSlotInput>
{
    public CreateTableSlotInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(30)
            .MustAsync(async (code, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                var normalized = code.Trim().ToUpperInvariant();
                return !await db.TableSlots.AnyAsync(x => x.Code == normalized, ct);
            })
            .WithMessage("Table code already exists");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(50);
    }
}
