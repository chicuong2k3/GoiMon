using FluentValidation;
using GoiMon.Api.Features.Authentication.Dtos;

namespace GoiMon.Api.Features.Authentication.Validators;

public class RegisterWithOAuthInputValidator : AbstractValidator<RegisterWithOAuthInput>
{
    public RegisterWithOAuthInputValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("OAuth token is required.")
            .MinimumLength(50).WithMessage("Token appears invalid.");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(p => p.Equals("Google", StringComparison.OrdinalIgnoreCase) || p.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Provider must be 'Google' or 'Facebook'.");

        RuleFor(x => x.OtpDeliveryMethod)
            .NotEmpty().WithMessage("OTP delivery method is required.");
        // Enum validation is automatic - GraphQL/HotChocolate handles invalid values
    }
}

public class LoginWithOAuthInputValidator : AbstractValidator<LoginWithOAuthInput>
{
    public LoginWithOAuthInputValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("OAuth token is required.")
            .MinimumLength(50).WithMessage("Token appears invalid.");

        RuleFor(x => x.Provider)
            .NotEmpty().WithMessage("Provider is required.")
            .Must(p => p.Equals("Google", StringComparison.OrdinalIgnoreCase) || p.Equals("Facebook", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Provider must be 'Google' or 'Facebook'.");

        RuleFor(x => x.OtpDeliveryMethod)
            .NotEmpty().WithMessage("OTP delivery method is required.");
        // Enum validation is automatic - GraphQL/HotChocolate handles invalid values
    }
}

public class VerifyOtpInputValidator : AbstractValidator<VerifyOtpInput>
{
    public VerifyOtpInputValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Valid user ID is required.");

        RuleFor(x => x.OtpToken)
            .NotEmpty().WithMessage("OTP token is required.")
            .Length(6).WithMessage("OTP must be exactly 6 digits.")
            .Matches(@"^\d{6}$").WithMessage("OTP must contain only digits.");
    }
}
