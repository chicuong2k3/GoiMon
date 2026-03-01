using System.Net.Http.Json;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;

namespace GoiMon.Api.Infrastructure.Services;

/// <summary>
/// OAuth user information returned from provider.
/// </summary>
public class OAuthUserInfo
{
    public required string UserId { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhotoUrl { get; set; }
}

/// <summary>
/// Interface for OAuth token exchange and validation.
/// </summary>
public interface IOAuthExchangeService
{
    /// <summary>
    /// Exchanges a Google ID token for user information.
    /// </summary>
    /// <param name="idToken">Google ID token from OAuth flow.</param>
    /// <returns>Normalized user information.</returns>
    Task<OAuthUserInfo> ExchangeGoogleTokenAsync(string idToken);

    /// <summary>
    /// Exchanges a Facebook access token for user information.
    /// </summary>
    /// <param name="accessToken">Facebook access token from OAuth flow.</param>
    /// <returns>Normalized user information.</returns>
    Task<OAuthUserInfo> ExchangeFacebookTokenAsync(string accessToken);
}

/// <summary>
/// Implements OAuth token exchange for Google and Facebook.
/// </summary>
public class OAuthExchangeService : IOAuthExchangeService
{
    private readonly ILogger<OAuthExchangeService> _logger;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public OAuthExchangeService(ILogger<OAuthExchangeService> logger, HttpClient httpClient, IConfiguration configuration)
    {
        _logger = logger;
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task<OAuthUserInfo> ExchangeGoogleTokenAsync(string idToken)
    {
        try
        {
            // Validate Google ID token using Google.Apis.Auth
            var settings = new GoogleJsonWebSignature.ValidationSettings();
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings).ConfigureAwait(false);

            _logger.LogInformation("Google token validated for user {Email}", payload.Email);

            return new OAuthUserInfo
            {
                UserId = payload.Subject,
                Email = payload.Email,
                FirstName = payload.GivenName ?? "Unknown",
                LastName = payload.FamilyName ?? "User",
                PhotoUrl = payload.Picture
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google token validation failed");
            throw new InvalidOperationException("Invalid Google ID token.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<OAuthUserInfo> ExchangeFacebookTokenAsync(string accessToken)
    {
        try
        {
            // Call Facebook Graph API to validate token and get user info
            var url = $"https://graph.facebook.com/me?fields=id,email,first_name,last_name,picture.width(200).height(200)&access_token={accessToken}";
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Facebook API returned {StatusCode}: {Content}", response.StatusCode, content);
                throw new InvalidOperationException($"Facebook API error: {response.StatusCode}");
            }

            var facebookUser = await response.Content.ReadFromJsonAsync<FacebookUserResponse>().ConfigureAwait(false);

            if (facebookUser?.Id == null || facebookUser.Email == null)
            {
                throw new InvalidOperationException("Missing required fields in Facebook response.");
            }

            _logger.LogInformation("Facebook token validated for user {Email}", facebookUser.Email);

            return new OAuthUserInfo
            {
                UserId = facebookUser.Id,
                Email = facebookUser.Email,
                FirstName = facebookUser.FirstName ?? "Unknown",
                LastName = facebookUser.LastName ?? "User",
                PhotoUrl = facebookUser.Picture?.Data?.Url
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facebook token validation failed");
            throw new InvalidOperationException("Invalid Facebook access token.", ex);
        }
    }

    /// <summary>
    /// Represents the Facebook Graph API user response.
    /// </summary>
    private class FacebookUserResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")]
        public string? Id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("email")]
        public string? Email { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("picture")]
        public PictureData? Picture { get; set; }
    }

    private class PictureData
    {
        [System.Text.Json.Serialization.JsonPropertyName("data")]
        public PictureInfo? Data { get; set; }
    }

    private class PictureInfo
    {
        [System.Text.Json.Serialization.JsonPropertyName("url")]
        public string? Url { get; set; }
    }
}
