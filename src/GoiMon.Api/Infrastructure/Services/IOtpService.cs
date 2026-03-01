using GoiMon.Api.Domain;

namespace GoiMon.Api.Infrastructure.Services;

/// <summary>
/// Interface for OTP (One-Time Password) service.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates and stores a new OTP token.
    /// </summary>
    /// <param name="userId">User ID to generate OTP for.</param>
    /// <param name="deliveryMethod">Email or Sms delivery method.</param>
    /// <param name="contextFactory">DB context factory for persistence.</param>
    /// <returns>The generated 6-digit OTP token.</returns>
    Task<string> GenerateOtpAsync(Guid userId, OtpDeliveryMethod deliveryMethod, IDbContextFactory<AppDbContext> contextFactory);

    /// <summary>
    /// Validates an OTP token for a user.
    /// </summary>
    /// <param name="userId">User ID to validate OTP for.</param>
    /// <param name="token">The OTP token to validate.</param>
    /// <param name="contextFactory">DB context factory for persistence.</param>
    /// <returns>Tuple of (IsValid, Message).</returns>
    Task<(bool IsValid, string Message)> ValidateOtpAsync(Guid userId, string token, IDbContextFactory<AppDbContext> contextFactory);

    /// <summary>
    /// Cleanup expired OTP tokens from the database.
    /// </summary>
    Task<int> InvalidateExpiredOtpsAsync(IDbContextFactory<AppDbContext> contextFactory);
}
