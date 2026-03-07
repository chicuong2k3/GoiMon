namespace GoiMon.Api.Features.Authentication.Dtos;

/// <summary>
/// User data transfer object.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhotoUrl { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }

    public static UserDto FromEntity(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhotoUrl = user.PhotoUrl,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }
}

/// <summary>
/// Response payload for authentication operations (register/login).
/// </summary>
public class AuthenticationPayload
{
    /// <summary>
    /// The authenticated user.
    /// </summary>
    public required UserDto User { get; set; }

    /// <summary>
    /// JWT token if user is verified, null if OTP verification is required.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// Whether OTP verification is required before token is issued.
    /// </summary>
    public bool RequiresOtpVerification { get; set; }

    /// <summary>
    /// Message to display to user.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Response payload for OTP verification.
/// </summary>
public class OtpVerificationPayload
{
    /// <summary>
    /// Whether OTP verification was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// JWT token if verification successful, null otherwise.
    /// </summary>
    public string? Token { get; set; }

    /// <summary>
    /// The verified user.
    /// </summary>
    public UserDto? User { get; set; }

    /// <summary>
    /// Message describing the result.
    /// </summary>
    public required string Message { get; set; } = string.Empty;
}
