namespace GoiMon.Portal.Features.Authentication;

public static class OAuthHelper
{
    public static string? ExtractTokenFromFragment(string fragment)
    {
        if (string.IsNullOrEmpty(fragment))
            return null;

        var query = fragment.TrimStart('#', '?');
        var parameters = ParseQueryString(query);

        if (parameters.TryGetValue("id_token", out var idToken) && !string.IsNullOrEmpty(idToken))
            return idToken;

        if (parameters.TryGetValue("access_token", out var accessToken) && !string.IsNullOrEmpty(accessToken))
            return accessToken;

        return null;
    }

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

    public static string GetFacebookAuthUrl(string appId, string redirectUri)
    {
        var scope = "email public_profile";
        return $"https://www.facebook.com/v20.0/dialog/oauth?" +
               $"client_id={Uri.EscapeDataString(appId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               $"&scope={Uri.EscapeDataString(scope)}" +
               $"&response_type=token";
    }

    private static Dictionary<string, string> ParseQueryString(string queryString)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(queryString))
            return result;

        foreach (var pair in queryString.Split('&'))
        {
            var eqIdx = pair.IndexOf('=');
            if (eqIdx <= 0) continue;
            result[Uri.UnescapeDataString(pair[..eqIdx])] = Uri.UnescapeDataString(pair[(eqIdx + 1)..]);
        }
        return result;
    }
}
