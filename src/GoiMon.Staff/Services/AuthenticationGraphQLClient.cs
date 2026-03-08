using GoiMon.Staff.Features.Authentication.Models;
using StrawberryShake;

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
    /// Verifies OTP code and returns the app JWT token on success.
    /// </summary>
    public async Task<(bool Success, string? Token, AuthenticationUser? User, string? Error)> VerifyOtpAsync(
        Guid userId,
        string otpCode)
    {
        try
        {
            _logger.LogInformation("Calling VerifyOtp mutation for user {UserId}", userId);
            var result = await _client.VerifyOtp.ExecuteAsync(userId, otpCode);

            if (result.IsErrorResult() || result.Data?.VerifyOtp is null)
            {
                var error = result.Errors.FirstOrDefault()?.Message ?? "OTP verification failed";
                _logger.LogWarning("VerifyOtp failed: {Error}", error);
                return (false, null, null, error);
            }

            var data = result.Data.VerifyOtp;
            if (!data.Success)
                return (false, null, null, data.Message);

            AuthenticationUser? user = null;
            if (data.User is not null)
            {
                user = new AuthenticationUser
                {
                    Email = data.User.Email,
                    DisplayName = $"{data.User.FirstName} {data.User.LastName}".Trim(),
                    IsVerified = data.User.IsVerified
                };
            }

            return (true, data.Token, user, null);
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
    public Guid UserId { get; set; }
    public string? Token { get; set; }
    public bool RequiresOtpVerification { get; set; }
    public string? Message { get; set; }
}
