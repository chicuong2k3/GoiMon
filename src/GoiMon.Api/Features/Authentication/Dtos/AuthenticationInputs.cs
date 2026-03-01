namespace GoiMon.Api.Features.Authentication.Dtos;

/// <summary>
/// Input for registering a user with OAuth provider.
/// </summary>
public class RegisterWithOAuthInput
{
    /// <summary>
    /// ID token from OAuth provider (Google) or access token (Facebook).
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// OAuth provider: "Google" or "Facebook".
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// Delivery method for OTP: Email or Sms.
    /// </summary>
    public required Domain.OtpDeliveryMethod OtpDeliveryMethod { get; set; }
}

/// <summary>
/// Input for logging in with OAuth provider.
/// </summary>
public class LoginWithOAuthInput
{
    /// <summary>
    /// ID token from OAuth provider (Google) or access token (Facebook).
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// OAuth provider: "Google" or "Facebook".
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// Delivery method for OTP if user not yet verified: Email or Sms.
    /// </summary>
    public required Domain.OtpDeliveryMethod OtpDeliveryMethod { get; set; }
}

/// <summary>
/// Input for verifying an OTP token.
/// </summary>
public class VerifyOtpInput
{
    /// <summary>
    /// User ID that the OTP is for.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// The 6-digit OTP token to verify.
    /// </summary>
    public required string OtpToken { get; set; }
}
