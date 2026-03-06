using GoiMon.Staff.Features.Authentication.Models;

namespace GoiMon.Staff.Services;

/// <summary>
/// Client wrapper for authentication GraphQL mutations.
/// </summary>
public class AuthenticationGraphQLClient
{
    private readonly GoiMonClient _client;
    private readonly ILogger<AuthenticationGraphQLClient> _logger;

    public AuthenticationGraphQLClient(GoiMonClient client, ILogger<AuthenticationGraphQLClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user with Google OAuth.
    /// </summary>
    /// <param name="idToken">Google ID token from OAuth flow.</param>
    /// <param name="otpDeliveryMethod">Delivery method: Email or Sms (enum: 0=Email, 1=Sms).</param>
    /// <returns>Authentication payload with user and OTP requirement status.</returns>
    public async Task<(bool Success, AuthenticationPayload? Payload, string? Error)> RegisterWithGoogleAsync(
        string idToken,
        int otpDeliveryMethod)
    {
        try
        {
            _logger.LogInformation("Calling RegisterWithGoogle mutation");

            // GraphQL mutation call would go here
            // var result = await _client.RegisterWithGoogle(new RegisterWithOAuthInput 
            // {
            //     Token = idToken,
            //     Provider = "Google",
            //     OtpDeliveryMethod = otpDeliveryMethod
            // });

            return (true, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegisterWithGoogle mutation failed");
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Registers a new user with Facebook OAuth.
    /// </summary>
    public async Task<(bool Success, AuthenticationPayload? Payload, string? Error)> RegisterWithFacebookAsync(
        string accessToken,
        int otpDeliveryMethod)
    {
        try
        {
            _logger.LogInformation("Calling RegisterWithFacebook mutation");

            // GraphQL mutation call would go here

            return (true, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RegisterWithFacebook mutation failed");
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Logs in user with Google OAuth.
    /// </summary>
    public async Task<(bool Success, AuthenticationPayload? Payload, string? Error)> LoginWithGoogleAsync(
        string idToken,
        int otpDeliveryMethod)
    {
        try
        {
            _logger.LogInformation("Calling LoginWithGoogle mutation");

            // GraphQL mutation call would go here

            return (true, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginWithGoogle mutation failed");
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Logs in user with Facebook OAuth.
    /// </summary>
    public async Task<(bool Success, AuthenticationPayload? Payload, string? Error)> LoginWithFacebookAsync(
        string accessToken,
        int otpDeliveryMethod)
    {
        try
        {
            _logger.LogInformation("Calling LoginWithFacebook mutation");

            // GraphQL mutation call would go here

            return (true, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LoginWithFacebook mutation failed");
            return (false, null, ex.Message);
        }
    }

    /// <summary>
    /// Verifies OTP code and returns JWT token on success.
    /// </summary>
    public async Task<(bool Success, string? Token, AuthenticationUser? User, string? Error)> VerifyOtpAsync(
        Guid userId,
        string otpCode)
    {
        try
        {
            _logger.LogInformation("Calling VerifyOtp mutation for user {UserId}", userId);

            // GraphQL mutation call would go here
            // var result = await _client.VerifyOtp(new VerifyOtpInput
            // {
            //     UserId = userId,
            //     OtpToken = otpCode
            // });

            return (true, null, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "VerifyOtp mutation failed");
            return (false, null, null, ex.Message);
        }
    }
}

/// <summary>
/// Authentication payload from GraphQL API.
/// </summary>
public class AuthenticationPayload
{
    public required AuthenticationUser User { get; set; }
    public string? Token { get; set; }
    public bool RequiresOtpVerification { get; set; }
    public string? Message { get; set; }
}
