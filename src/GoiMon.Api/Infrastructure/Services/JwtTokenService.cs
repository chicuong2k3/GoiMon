using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace GoiMon.Api.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly ILogger<JwtTokenService> _logger;
    private readonly string _signingKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryMinutes;

    public JwtTokenService(ILogger<JwtTokenService> logger, IConfiguration configuration)
    {
        _logger = logger;
        var jwtConfig = configuration.GetSection("Jwt");
        _signingKey = jwtConfig["SigningKey"] ?? throw new InvalidOperationException("Jwt:SigningKey not configured");
        _issuer = jwtConfig["Issuer"] ?? "goimon-api";
        _audience = jwtConfig["Audience"] ?? "goimon-client";
        _expiryMinutes = int.TryParse(jwtConfig["ExpiryMinutes"], out var expiry) ? expiry : 1440; // 24 hours default
    }

    /// <inheritdoc />
    public string GenerateToken(Guid userId, string email, string role, bool isVerified = true)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("verified", isVerified.ToString().ToLower()),
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expiryMinutes),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        _logger.LogInformation("JWT token generated for UserId={UserId}, Email={Email}, ExpiresIn={Minutes} minutes",
            userId, email, _expiryMinutes);

        return tokenString;
    }

    /// <inheritdoc />
    public (bool IsValid, Guid? UserId, string? Email, bool IsVerified, string? Role) ValidateToken(string token)
    {
        try
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out var validatedToken);

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value;
            var verifiedClaim = principal.FindFirst("verified")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Token validation failed: Invalid userId claim");
                return (false, null, null, false, null);
            }

            var isVerified = bool.TryParse(verifiedClaim, out var verified) && verified;
            var roleClaim = principal.FindFirst(ClaimTypes.Role)?.Value;

            _logger.LogInformation("JWT token validated for UserId={UserId}", userId);

            return (true, userId, emailClaim, isVerified, roleClaim);
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
            return (false, null, null, false, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token validation error: {Message}", ex.Message);
            return (false, null, null, false, null);
        }
    }
}
