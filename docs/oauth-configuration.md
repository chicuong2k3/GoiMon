# OAuth Configuration — Google and Facebook

This document describes the exact settings needed to configure Google and Facebook OAuth for the GoiMon project (API + Staff frontend).

Summary
- API: `src/GoiMon.Api/appsettings.json` holds server-side secrets (ClientSecret / AppSecret) and canonical redirect URIs.
- Staff (frontend): `src/GoiMon.Staff/wwwroot/appsettings.json` holds `ClientId` / `AppId` and the `RedirectUri` the SPA will use.

1) Google (Google Cloud Console)
- Create an OAuth 2.0 Client ID (type: "Web application" or "Single-page application" depending on deployment).
- Required values to configure in Google Console:
  - Authorized redirect URIs:
    - `http://localhost:5001/authentication/google-callback` (development)
    - Add your production redirect URI (e.g., `https://app.example.com/authentication/google-callback`).
  - For SPAs you may also set Authorized JavaScript origins to your app origin (e.g., `http://localhost:5001`).
- Scopes: use `openid email profile`.

2) Facebook (Facebook Developers)
- Create an App and enable Facebook Login.
- Required values to configure in Facebook App settings:
  - OAuth redirect URI / Valid OAuth Redirect URIs:
    - `http://localhost:5001/authentication/facebook-callback` (development)
    - Add production redirect URI if applicable.
  - App Domains: add the host (e.g., `localhost` for dev or `example.com` for prod).

3) Local appsettings examples

- API (`src/GoiMon.Api/appsettings.json`)

{
  "OAuth": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET",
      "RedirectUri": "http://localhost:5001/authentication/google-callback"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET",
      "RedirectUri": "http://localhost:5001/authentication/facebook-callback"
    }
  }
}

- Staff (frontend) (`src/GoiMon.Staff/wwwroot/appsettings.json`)

{
  "OAuth": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com",
      "RedirectUri": "http://localhost:5001/authentication/google-callback"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "RedirectUri": "http://localhost:5001/authentication/facebook-callback"
    }
  }
}

4) How the flow works (overview)
- Frontend (SPA) redirects the browser to provider's auth URL (Google / Facebook) with `redirect_uri` set to the SPA callback URL.
- Provider returns a token (ID token or access token) to the SPA via the fragment or query string.
- SPA extracts the token and calls API GraphQL mutations (`registerWithGoogle`, `loginWithGoogle`, etc.) passing the provider token to the server.
- Server verifies token with provider (via `OAuthExchangeService`) and proceeds with registration/login, including OTP generation/verification.

5) Security notes
- Keep `ClientSecret` / `AppSecret` only on the server (`src/GoiMon.Api/appsettings.json`) and never commit real secrets to source control.
- For production, use a secure secret store (Azure Key Vault, AWS Secrets Manager, etc.) and environment variables rather than checked-in appsettings.
- Ensure the redirect URIs configured in provider consoles exactly match the URIs used by your frontend (including scheme and port).

6) Troubleshooting
- "redirect_uri_mismatch": The URL sent by the client does not exactly match the value registered in the provider console.
- Ensure `response_type` and `scope` values are correct for the chosen provider (Google requires `id_token`/`token` for implicit flows).

If you want, I can:
- Add sample production-ready `appsettings.Production.json` entries (without secrets) and a small checklist for registering the apps in each provider.
- Wire the unified authentication page to call the GraphQL register/login mutations next.

*** End of document
