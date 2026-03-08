namespace GoiMon.Staff.Features.Authentication.Helpers;

/// <summary>
/// Helper methods for OAuth 2.0 token extraction and handling.
/// </summary>
public static class OAuthHelper
{
    /// <summary>
    /// Extracts access/ID token from OAuth callback URL fragment.
    /// </summary>
    /// <param name="fragment">URL fragment containing OAuth response (e.g., access_token=XXX#...)</param>
    /// <returns>Token if found, null otherwise.</returns>
    public static string? ExtractTokenFromFragment(string fragment)
    {
        if (string.IsNullOrEmpty(fragment))
            return null;

        // OAuth responses can be in hash: #access_token=xxx&... or ?access_token=xxx&...
        var query = fragment.TrimStart('#', '?');

        // Parse query string parameters
        var parameters = ParseQueryString(query);

        // Prefer id_token (JWT signed by provider) over access_token (opaque bearer token)
        if (parameters.TryGetValue("id_token", out var idToken) && !string.IsNullOrEmpty(idToken))
            return idToken;

        if (parameters.TryGetValue("access_token", out var accessToken) && !string.IsNullOrEmpty(accessToken))
            return accessToken;

        if (parameters.TryGetValue("auth_token", out var authToken) && !string.IsNullOrEmpty(authToken))
            return authToken;

        return null;
    }

    /// <summary>
    /// Gets authorization header value with JWT token.
    /// </summary>
    /// <param name="token">JWT token from API.</param>
    /// <returns>Authorization header value (e.g., "Bearer {token}").</returns>
    public static string GetAuthorizationHeader(string token)
    {
        return $"Bearer {token}";
    }

    /// <summary>
    /// Parses query string into key-value pairs.
    /// </summary>
    private static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>();

        if (string.IsNullOrEmpty(queryString))
            return result;

        var pairs = queryString.Split('&');
        foreach (var pair in pairs)
        {
            var eqIdx = pair.IndexOf('=');
            if (eqIdx <= 0) continue;
            var key = Uri.UnescapeDataString(pair[..eqIdx]);
            var value = Uri.UnescapeDataString(pair[(eqIdx + 1)..]);
            result[key] = value;
        }

        return result;
    }

    /// <summary>
    /// Generates Google OAuth login URL.
    /// </summary>
    /// <param name="clientId">Google Client ID from console.cloud.google.com</param>
    /// <param name="redirectUri">Redirect URI (must match Google Console config)</param>
    /// <returns>URL to redirect user to Google login.</returns>
    public static string GetGoogleAuthUrl(string clientId, string redirectUri)
    {
        var scope = "openid email profile";
        var responseType = "id_token token";
        return $"https://accounts.google.com/o/oauth2/v2/auth?" +
               $"client_id={Uri.EscapeDataString(clientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&response_type={Uri.EscapeDataString(responseType)}" +
               $"&scope={Uri.EscapeDataString(scope)}" +
               $"&nonce={Guid.NewGuid()}";
    }

    /// <summary>
    /// Generates Facebook OAuth login URL.
    /// </summary>
    /// <param name="appId">Facebook App ID from developers.facebook.com</param>
    /// <param name="redirectUri">Redirect URI (must match Facebook app config)</param>
    /// <returns>URL to redirect user to Facebook login.</returns>
    public static string GetFacebookAuthUrl(string appId, string redirectUri)
    {
        var scope = "email public_profile";
        return $"https://www.facebook.com/v20.0/dialog/oauth?" +
               $"client_id={Uri.EscapeDataString(appId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&scope={Uri.EscapeDataString(scope)}" +
               $"&response_type=token";
    }
}
