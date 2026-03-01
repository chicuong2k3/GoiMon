namespace GoiMon.Api.Domain.Entities;

public sealed class User : IAggregateRoot
{
    /// <summary>
    /// Unique identifier for the user.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address (unique, indexed).
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's phone number (nullable, for SMS OTP delivery).
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// User's first name.
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name.
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Google OAuth ID (nullable, unique if set).
    /// </summary>
    public string? GoogleId { get; set; }

    /// <summary>
    /// Facebook OAuth ID (nullable, unique if set).
    /// </summary>
    public string? FacebookId { get; set; }

    /// <summary>
    /// Profile photo URL (from OAuth provider).
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Indicates if user has completed OTP verification.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Indicates if user account is active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Timestamp when user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp of last update.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Navigation property: related OTP tokens.
    /// </summary>
    public ICollection<OtpToken> OtpTokens { get; set; } = new List<OtpToken>();

    public User() { }

    public User(Guid id, string email, string? firstName = null, string? lastName = null)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates a user from OAuth provider information.
    /// </summary>
    public static User CreateFromOAuth(Guid id, string email, string? firstName = null, string? lastName = null, string? photoUrl = null, string? googleId = null, string? facebookId = null)
    {
        return new User
        {
            Id = id,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            GoogleId = googleId,
            FacebookId = facebookId,
            PhotoUrl = photoUrl,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks user as verified after OTP confirmation.
    /// </summary>
    public void MarkAsVerified()
    {
        IsVerified = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
