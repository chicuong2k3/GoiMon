namespace GoiMon.Client.Features.Authentication.Models;

/// <summary>
/// Represents an authenticated user in the application.
/// </summary>
public class AuthenticationUser
{
    /// <summary>
    /// Unique user identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// User's display name (FirstName + LastName or Email if not set).
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// User's first name (optional).
    /// </summary>
    public string? FirstName => GetDisplayName();

    /// <summary>
    /// User's last name (optional).
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// User's profile photo URL.
    /// </summary>
    public string? PhotoUrl { get; set; }

    /// <summary>
    /// Whether user has completed verification.
    /// </summary>
    public bool IsVerified { get; set; }

    /// <summary>
    /// Local time when user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    public AuthenticationUser()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.Now;
    }

    /// <summary>
    /// Gets display name, falling back to email if FirstName/LastName not set.
    /// </summary>
    public string GetDisplayName()
    {
        if (!string.IsNullOrEmpty(FirstName) || !string.IsNullOrEmpty(LastName))
        {
            return $"{FirstName ?? ""} {LastName ?? ""}".Trim();
        }

        return Email;
    }
}
