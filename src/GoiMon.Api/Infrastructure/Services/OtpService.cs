using System.Security.Cryptography;
using GoiMon.Api.Domain;
using GoiMon.Api.Infrastructure.Email;

namespace GoiMon.Api.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly ILogger<OtpService> _logger;
    private readonly IEmailService _emailService;
    private const int OtpLength = 6;
    private const int DefaultExpiryMinutes = 10;

    public OtpService(ILogger<OtpService> logger, IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    /// <inheritdoc />
    public async Task<string> GenerateOtpAsync(Guid userId, OtpDeliveryMethod deliveryMethod, IDbContextFactory<AppDbContext> contextFactory)
    {
        var token = GenerateRandomOtp();

        using var context = contextFactory.CreateDbContext();

        // OTP operations happen before the user is authenticated — bypass the global TenantId filter.
        var user = context.Users.IgnoreQueryFilters().FirstOrDefault(u => u.Id == userId);
        if (user is null)
        {
            _logger.LogWarning("Cannot generate OTP: user {UserId} not found", userId);
            return token;
        }

        // Invalidate any existing non-expired OTPs for this user
        var existingOtps = context.OtpTokens
            .IgnoreQueryFilters()
            .Where(o => o.UserId == userId && !o.IsUsed)
            .ToList();

        foreach (var oldOtp in existingOtps)
            oldOtp.IsUsed = true;

        var otpToken = new OtpToken
        {
            Id = Guid.NewGuid(),
            TenantId = user.TenantId,
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

        _logger.LogInformation(
            "OTP generated for UserId={UserId}, DeliveryMethod={Method}, ExpiresAt={ExpiresAt}",
            userId, deliveryMethod, otpToken.ExpiresAt);

        await SendOtpAsync(user, deliveryMethod, token).ConfigureAwait(false);

        return token;
    }

    /// <inheritdoc />
    public async Task<(bool IsValid, string Message)> ValidateOtpAsync(Guid userId, string token, IDbContextFactory<AppDbContext> contextFactory)
    {
        using var context = contextFactory.CreateDbContext();

        // OTP validation happens before the user is authenticated — bypass the global TenantId filter.
        var otpToken = context.OtpTokens
            .IgnoreQueryFilters()
            .FirstOrDefault(o => o.UserId == userId && o.Token == token && !o.IsUsed);

        if (otpToken == null)
        {
            _logger.LogWarning("OTP validation failed for UserId={UserId}: Token not found or already used", userId);
            return (false, "Invalid OTP token.");
        }

        if (otpToken.IsExpired())
        {
            _logger.LogWarning("OTP validation failed for UserId={UserId}: Token expired", userId);
            return (false, "OTP token has expired.");
        }

        if (otpToken.FailedAttempts >= OtpToken.MaxFailedAttempts)
        {
            _logger.LogWarning("OTP validation failed for UserId={UserId}: Max attempts exceeded", userId);
            return (false, "OTP token locked due to too many failed attempts.");
        }

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

        // Background job — no tenant context, must bypass the global TenantId filter.
        var expiredTokens = context.OtpTokens
            .IgnoreQueryFilters()
            .Where(o => !o.IsUsed && o.ExpiresAt <= DateTime.UtcNow)
            .ToList();

        if (expiredTokens.Count == 0)
            return 0;

        foreach (var t in expiredTokens)
            t.IsUsed = true;

        context.OtpTokens.UpdateRange(expiredTokens);
        var deleted = await context.SaveChangesAsync().ConfigureAwait(false);

        _logger.LogInformation("Invalidated {Count} expired OTP tokens", expiredTokens.Count);
        return deleted;
    }

    private async Task SendOtpAsync(User user, OtpDeliveryMethod deliveryMethod, string token)
    {
        if (deliveryMethod == OtpDeliveryMethod.Email)
        {
            try
            {
                await _emailService.SendOtpAsync(user.Email, token, DefaultExpiryMinutes);
            }
            catch (Exception ex)
            {
                // Log but don't fail the OTP generation — token is already saved
                _logger.LogError(ex, "OTP email delivery failed for user {UserId}. Token was generated and saved.", user.Id);
            }
        }
        else if (deliveryMethod == OtpDeliveryMethod.Sms)
        {
            // TODO: integrate SMS provider (Twilio, etc.)
            _logger.LogWarning("[SMS NOT IMPLEMENTED] OTP for user {UserId}: {Token}", user.Id, token);
        }
    }

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
}
