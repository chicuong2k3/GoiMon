using GoiMon.Api.Domain;

namespace GoiMon.Api.Domain.Entities;

/// <summary>
/// Represents a One-Time Password token for user verification.
/// </summary>
public sealed class OtpToken : IMultiTenant
{
    /// <summary>
    /// Unique identifier for the OTP token.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key: Tenant this OTP belongs to.
    /// </summary>
    public Guid TenantId { get; set; }

    /// <summary>
    /// Foreign key: User this OTP is for.
    /// </summary>
    public required Guid UserId { get; set; }

    /// <summary>
    /// Navigation property: User this OTP is for.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// The 6-digit OTP code.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// Delivery method for this OTP: Email or Sms.
    /// </summary>
    public required OtpDeliveryMethod DeliveryMethod { get; set; }

    /// <summary>
    /// Timestamp when token expires (typically 10 minutes from creation).
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether this OTP token has been successfully used.
    /// </summary>
    public bool IsUsed { get; set; }

    /// <summary>
    /// Timestamp of creation.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when token was used (if applicable).
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Number of failed verification attempts.
    /// </summary>
    public int FailedAttempts { get; set; }

    /// <summary>
    /// Maximum allowed failed attempts before token is locked.
    /// </summary>
    public const int MaxFailedAttempts = 3;

    public OtpToken() { }

    /// <summary>
    /// Checks if OTP is still valid (not expired and not already used).
    /// </summary>
    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow < ExpiresAt && FailedAttempts < MaxFailedAttempts;
    }

    /// <summary>
    /// Checks if OTP has expired.
    /// </summary>
    public bool IsExpired()
    {
        return DateTime.UtcNow >= ExpiresAt;
    }

    /// <summary>
    /// Records a failed verification attempt.
    /// </summary>
    public void RecordFailedAttempt()
    {
        FailedAttempts++;
    }

    /// <summary>
    /// Marks this token as used.
    /// </summary>
    public void MarkAsUsed()
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}
