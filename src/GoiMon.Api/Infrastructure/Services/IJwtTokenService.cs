namespace GoiMon.Api.Infrastructure.Services;

/// <summary>
/// Interface for JWT token generation and validation.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a new JWT token for a user.
    /// </summary>
    /// <param name="userId">User ID to encode in token.</param>
    /// <param name="tenantId">Tenant ID to encode in token.</param>
    /// <param name="email">User email to encode in token.</param>
    /// <param name="isVerified">Whether user has completed OTP verification.</param>
    /// <param name="role">User role to encode in token.</param>
    /// <returns>JWT token string.</returns>
    string GenerateToken(Guid userId, Guid tenantId, string email, string role, bool isVerified = true);

    /// <summary>
    /// Validates and parses a JWT token.
    /// </summary>
    /// <param name="token">JWT token to validate.</param>
    /// <returns>Tuple of (IsValid, UserId, TenantId, Email, IsVerified, Role).</returns>
    (bool IsValid, Guid? UserId, Guid? TenantId, string? Email, bool IsVerified, string? Role) ValidateToken(string token);
}
