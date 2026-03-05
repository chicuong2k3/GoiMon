using FluentValidation;
using GoiMon.Api.Features.Tables.Models;

namespace GoiMon.Api.Features.Tables.Validators;

public sealed class UpdateTableSlotInputValidator : AbstractValidator<UpdateTableSlotInput>
{
    public UpdateTableSlotInputValidator(IDbContextFactory<AppDbContext> dbFactory)
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(30)
            .MustAsync(async (input, code, ct) =>
            {
                using var db = dbFactory.CreateDbContext();
                var normalized = code.Trim().ToUpperInvariant();
                return !await db.TableSlots.AnyAsync(x => x.Id != input.Id && x.Code == normalized, ct);
            })
            .WithMessage("Table code already exists");

        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Capacity).GreaterThan(0).LessThanOrEqualTo(50);
    }
}
