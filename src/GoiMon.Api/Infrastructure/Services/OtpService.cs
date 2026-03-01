using System.Security.Cryptography;
using GoiMon.Api.Domain;

namespace GoiMon.Api.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly ILogger<OtpService> _logger;
    private const int OtpLength = 6;
    private const int DefaultExpiryMinutes = 10;

    public OtpService(ILogger<OtpService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<string> GenerateOtpAsync(Guid userId, OtpDeliveryMethod deliveryMethod, IDbContextFactory<AppDbContext> contextFactory)
    {
        // Generate random 6-digit OTP
        var token = GenerateRandomOtp();

        using var context = contextFactory.CreateDbContext();

        // Invalidate any existing non-expired OTPs for this user
        var existingOtps = context.OtpTokens
            .Where(o => o.UserId == userId && !o.IsUsed)
            .ToList();

        foreach (var oldOtp in existingOtps)
        {
            oldOtp.IsUsed = true;
        }

        // Create new OTP token
        var otpToken = new OtpToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = token,
            DeliveryMethod = deliveryMethod,
            ExpiresAt = DateTime.UtcNow.AddMinutes(DefaultExpiryMinutes),
            CreatedAt = DateTime.UtcNow,
            IsUsed = false,
            FailedAttempts = 0
        };

        context.OtpTokens.Add(otpToken);
        await context.SaveChangesAsync().ConfigureAwait(false);

        // Log OTP generation (without actual token in production)
        _logger.LogInformation(
            "OTP generated for UserId={UserId}, DeliveryMethod={Method}, ExpiresAt={ExpiresAt}",
            userId, deliveryMethod, otpToken.ExpiresAt);

        // Simulate sending OTP (placeholder)
        await SendOtpAsync(deliveryMethod, token, userId).ConfigureAwait(false);

        return token;
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, string Message)> ValidateOtpAsync(Guid userId, string token, IDbContextFactory<AppDbContext> contextFactory)
    {
        using var context = contextFactory.CreateDbContext();

        var otpToken = context.OtpTokens
            .FirstOrDefault(o => o.UserId == userId && o.Token == token && !o.IsUsed);

        if (otpToken == null)
        {
            _logger.LogWarning("OTP validation failed for UserId={UserId}: Token not found or already used", userId);
            return (false, "Invalid OTP token.");
        }

        // Check if expired
        if (otpToken.IsExpired())
        {
            _logger.LogWarning("OTP validation failed for UserId={UserId}: Token expired", userId);
            return (false, "OTP token has expired.");
        }

        // Check if max attempts exceeded
        if (otpToken.FailedAttempts >= OtpToken.MaxFailedAttempts)
        {
            _logger.LogWarning("OTP validation failed for UserId={UserId}: Max attempts exceeded", userId);
            return (false, "OTP token locked due to too many failed attempts.");
        }

        // Mark as used
        otpToken.MarkAsUsed();
        context.OtpTokens.Update(otpToken);
        await context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("OTP successfully validated for UserId={UserId}", userId);
        return (true, "OTP verified successfully.");
    }

    /// <inheritdoc />
    public async Task<int> InvalidateExpiredOtpsAsync(IDbContextFactory<AppDbContext> contextFactory)
    {
        using var context = contextFactory.CreateDbContext();

        var expiredTokens = context.OtpTokens
            .Where(o => !o.IsUsed && o.ExpiresAt <= DateTime.UtcNow)
            .ToList();

        if (expiredTokens.Count == 0)
        {
            return 0;
        }

        foreach (var token in expiredTokens)
        {
            token.IsUsed = true;
        }

        context.OtpTokens.UpdateRange(expiredTokens);
        var deleted = await context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Invalidated {Count} expired OTP tokens", expiredTokens.Count);
        return deleted;
    }

    /// <summary>
    /// Generates a random 6-digit numeric OTP.
    /// </summary>
    private static string GenerateRandomOtp()
    {
        var buffer = new byte[4];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        var random = new Random(BitConverter.ToInt32(buffer, 0));
        return random.Next(100000, 999999).ToString();
    }

    /// <summary>
    /// Simulates sending OTP via email or SMS.
    /// In production, integrate with SendGrid, Twilio, or similar.
    /// </summary>
    private Task SendOtpAsync(OtpDeliveryMethod deliveryMethod, string token, Guid userId)
    {
        if (deliveryMethod == OtpDeliveryMethod.Email)
        {
            _logger.LogInformation("📧 [PLACEHOLDER] Sending OTP via email for user {UserId}: {Token}", userId, token);
        }
        else if (deliveryMethod == OtpDeliveryMethod.Sms)
        {
            _logger.LogInformation("📱 [PLACEHOLDER] Sending OTP via SMS for user {UserId}: {Token}", userId, token);
        }

        return Task.CompletedTask;
    }
}
