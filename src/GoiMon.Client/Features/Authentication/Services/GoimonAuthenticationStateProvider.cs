using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using GoiMon.Client.Features.Authentication.Models;

namespace GoiMon.Client.Features.Authentication.Services;

/// <summary>
/// Custom AuthenticationStateProvider for GoiMon that manages JWT-based authentication.
/// </summary>
public class GoimonAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStorageService _tokenStorage;
    private readonly ILogger<GoimonAuthenticationStateProvider> _logger;
    private AuthenticationUser? _currentUser;

    public GoimonAuthenticationStateProvider(ITokenStorageService tokenStorage, ILogger<GoimonAuthenticationStateProvider> logger)
    {
        _tokenStorage = tokenStorage;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current authentication state of the user.
    /// </summary>
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenStorage.GetTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogInformation("No authentication token found");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            // Parse JWT to extract claims
            var claims = ParseJwtClaims(token);

            if (claims.Count == 0)
            {
                _logger.LogWarning("Failed to parse JWT claims");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            _logger.LogInformation("User authenticated: {Email}", user.FindFirst(ClaimTypes.Email)?.Value);

            return new AuthenticationState(user);
        }
        catch
        {
            _logger.LogError("Error retrieving authentication state");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
    /// Marks user as authenticated after successful login/registration.
    /// </summary>
    public async Task LoginAsync(string token, AuthenticationUser user)
    {
        await _tokenStorage.SetTokenAsync(token);
        _currentUser = user;

        var claims = ParseJwtClaims(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        var principal = new ClaimsPrincipal(identity);

        _logger.LogInformation("User logged in: {Email}", user.Email);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
    }

    /// <summary>
    /// Marks user as unauthenticated (logout).
    /// </summary>
    public async Task LogoutAsync()
    {
        await _tokenStorage.ClearTokenAsync();
        _currentUser = null;

        _logger.LogInformation("User logged out");

        NotifyAuthenticationStateChanged(Task.FromResult(
            new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))
        ));
    }

    /// <summary>
    /// Gets the current authenticated user.
    /// </summary>
    public AuthenticationUser? GetCurrentUser() => _currentUser;

    /// <summary>
    /// Parses JWT token to extract claims.
    /// </summary>
    private static List<Claim> ParseJwtClaims(string token)
    {
        var claims = new List<Claim>();

        try
        {
            // JWT format: header.payload.signature
            var parts = token.Split('.');
            if (parts.Length != 3)
                return claims;

            var payload = parts[1];

            // Add padding if needed
            var padding = 4 - (payload.Length % 4);
            if (padding != 4)
                payload += new string('=', padding);

            var decodedBytes = Convert.FromBase64String(payload);
            var decodedString = System.Text.Encoding.UTF8.GetString(decodedBytes);

            using var doc = JsonDocument.Parse(decodedString);
            var root = doc.RootElement;

            // Extract standard and custom claims
            foreach (var property in root.EnumerateObject())
            {
                var claimType = ConvertJsonPropertyNameToClaimType(property.Name);
                var claimValue = property.Value.GetString() ?? property.Value.ToString();

                claims.Add(new Claim(claimType, claimValue));
            }
        }
        catch (Exception ex)
        {
            // Log or handle parsing error
            return new List<Claim>();
        }

        return claims;
    }

    /// <summary>
    /// Converts JSON property names to standard claim types.
    /// </summary>
    private static string ConvertJsonPropertyNameToClaimType(string jsonPropertyName)
    {
        return jsonPropertyName switch
        {
            "sub" => ClaimTypes.NameIdentifier,
            "email" => ClaimTypes.Email,
            "name" => ClaimTypes.Name,
            "verified" => "verified",
            _ => jsonPropertyName
        };
    }
}
