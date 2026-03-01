# GoiMon Authentication - OAuth Provider Setup Guide

**Last Updated:** 2026-03-01  
**Version:** 1.0

---

## Overview

This guide explains how to set up OAuth authentication providers (Google and Facebook) for the GoiMon authentication system. The API uses OAuth tokens to register and authenticate users, followed by OTP verification.

---

## Prerequisites

- GoiMon.Api deployed or running locally
- Admin access to Google Cloud Console
- Admin access to Facebook Developer Portal
- Configuration file access (`appsettings.json`)

---

## Google OAuth Setup

### Step 1: Create a Google Cloud Project

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Click the project dropdown at the top
3. Click **NEW PROJECT**
4. Enter project name: `GoiMon` (or your preferred name)
5. Click **CREATE**
6. Wait for the project to be created

### Step 2: Enable Google OAuth 2.0 API

1. In the Google Cloud Console, navigate to **APIs & Services** > **Library**
2. Search for **"Google+ API"** or **"Identity and Access Management"**
3. Click on **Google+ API** or use **Identity Toolkit API** for OAuth
4. Click **ENABLE**
5. Search for **"OAuth Consent Screen"** and enable it

### Step 3: Create OAuth 2.0 Credentials

1. Go to **APIs & Services** > **Credentials**
2. Click **+ CREATE CREDENTIALS** > **OAuth client ID**
3. You may be prompted to configure the OAuth consent screen first:
   - Choose **External** as the user type
   - Fill in required fields (app name, user support email, etc.)
   - Click **SAVE AND CONTINUE**
4. After configuring consent screen, create the OAuth client ID:
   - **Application type:** Web application
   - **Name:** GoiMon Client
   - **Authorized redirect URIs:** Add:
     - `http://localhost:5000/auth/google/callback` (development)
     - `http://localhost:5002/auth/google/callback` (if using different port)
     - `https://yourdomain.com/auth/google/callback` (production)
   - Click **CREATE**
5. Copy the **Client ID** (you won't need the Client Secret for Google ID token validation)

### Step 4: Update GoiMon Configuration

Add to `appsettings.json`:

```json
{
  "OAuth": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE.apps.googleusercontent.com"
    }
  }
}
```

Or set environment variable:
```bash
export Oauth__Google__ClientId="YOUR_GOOGLE_CLIENT_ID_HERE.apps.googleusercontent.com"
```

---

## Facebook OAuth Setup

### Step 1: Create a Facebook App

1. Go to [Meta for Developers](https://developers.facebook.com/)
2. Click **My Apps** (top right)
3. Click **Create App**
4. Choose **Consumer** as the app type
5. Fill in basic information:
   - **App Name:** GoiMon
   - **App Contact Email:** your-email@example.com
   - **App Purpose:** Select "Allow marketers to use Facebook tools"
6. Click **Create App**

### Step 2: Configure Facebook Login

1. In your app dashboard, click **+ Add Product**
2. Find **Facebook Login** card
3. Click **Set Up**
4. Choose **Web** as your platform
5. You'll go through setup steps:
   - Fill in your site URL: `http://localhost:5000` (dev) or `https://yourdomain.com` (prod)
   - Continue through the guide

### Step 3: Get App Credentials

1. Go to **Settings** > **Basic**
2. Copy:
   - **App ID**
   - **App Secret** (keep this secure!)
3. Go to **Products** > **Facebook Login** > **Settings**
4. Set Valid OAuth Redirect URIs:
   - `http://localhost:5000/auth/facebook/callback`
   - `https://yourdomain.com/auth/facebook/callback`

### Step 4: Update GoiMon Configuration

Add to `appsettings.json`:

```json
{
  "OAuth": {
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
    }
  }
}
```

Or set environment variables:
```bash
export Oauth__Facebook__AppId="YOUR_FACEBOOK_APP_ID"
export Oauth__Facebook__AppSecret="YOUR_FACEBOOK_APP_SECRET"
```

---

## JWT Configuration

GoiMon uses JWT (JSON Web Tokens) for session management after OTP verification.

### Update JWT Settings in appsettings.json

```json
{
  "Jwt": {
    "SigningKey": "your-super-secret-key-at-least-32-characters-long-for-production",
    "Issuer": "goimon-api",
    "Audience": "goimon-client",
    "ExpiryMinutes": 1440
  }
}
```

**Secure Key Generation:**

```bash
# Generate a random secret key (Linux/Mac)
openssl rand -base64 32

# Or using .NET
dotnet user-secrets set "Jwt:SigningKey" "generated-secret-here"
```

**Important:** In production, use a strong, random key stored securely (AWS Secrets Manager, Azure Key Vault, etc.)

---

## OTP Configuration

The system sends verification codes via email or SMS (currently uses placeholders).

### Update OTP Settings in appsettings.json

```json
{
  "Otp": {
    "ExpiryMinutes": 10,
    "MaxAttempts": 3,
    "EmailProvider": "placeholder",
    "SmsProvider": "placeholder"
  }
}
```

### Integration with Email/SMS Providers

To use real email/SMS delivery, replace the placeholders in `OtpService.cs` with your provider's SDK:

**Email Examples:**
- SendGrid: `using SendGrid; using SendGrid.Helpers.Mail;`
- AWS SES: `using Amazon.SimpleEmail;`
- Twilio: `using Twilio;` (for SMS)

Update the `SendOtpAsync` method in `OtpService.cs`:

```csharp
private async Task SendOtpAsync(string deliveryMethod, string token, Guid userId)
{
    if (deliveryMethod.Equals("email", StringComparison.OrdinalIgnoreCase))
    {
        // Integration example: SendGrid
        // var client = new SendGridClient(apiKey);
        // var email = new SendGridMessage()
        // {
        //     From = new EmailAddress("noreply@goimon.com"),
        //     Subject = $"Your GoiMon Verification Code: {token}",
        //     HtmlContent = $"<p>Your code is: <strong>{token}</strong></p>"
        // };
        // await client.SendEmailAsync(email);
        
        _logger.LogInformation("📧 [SENDING EMAIL] OTP for user {UserId}: {Token}", userId, token);
    }
    else if (deliveryMethod.Equals("sms", StringComparison.OrdinalIgnoreCase))
    {
        // Integration example: Twilio
        // var twilio = new TwilioClient(accountSid, authToken);
        // await twilio.Messages.CreateAsync(
        //     body: $"GoiMon verification code: {token}",
        //     from: new Twilio.Types.PhoneNumber("+1XXXXXXXXXX"),
        //     to: new Twilio.Types.PhoneNumber(userPhone)
        // );
        
        _logger.LogInformation("📱 [SENDING SMS] OTP for user {UserId}: {Token}", userId, token);
    }
}
```

---

## Testing the Authentication Flow

### 1. Register with Google

```graphql
mutation RegisterGoogle {
  registerWithGoogle(input: {
    token: "GOOGLE_ID_TOKEN_HERE"
    provider: "Google"
    otpDeliveryMethod: "email"
  }) {
    user {
      id
      email
      firstName
      lastName
      isVerified
    }
    token
    requiresOtpVerification
    message
  }
}
```

### 2. Register with Facebook

```graphql
mutation RegisterFacebook {
  registerWithFacebook(input: {
    token: "FACEBOOK_ACCESS_TOKEN_HERE"
    provider: "Facebook"
    otpDeliveryMethod: "email"
  }) {
    user {
      id
      email
      firstName
      lastName
      isVerified
    }
    token
    requiresOtpVerification
    message
  }
}
```

### 3. Verify OTP

```graphql
mutation VerifyOtp {
  verifyOtp(input: {
    userId: "USER_ID_FROM_REGISTER"
    otpToken: "123456"
  }) {
    success
    token
    user {
      id
      email
      firstName
      lastName
      isVerified
    }
    message
  }
}
```

### 4. Login with Google (Already Verified User)

```graphql
mutation LoginGoogle {
  loginWithGoogle(input: {
    token: "GOOGLE_ID_TOKEN_HERE"
    provider: "Google"
    otpDeliveryMethod: "email"
  }) {
    user {
      id
      email
      firstName
      lastName
      isVerified
    }
    token
    requiresOtpVerification
    message
  }
}
```

---

## Environment Variables Summary

Create a `.env` file or add to your deployment platform:

```bash
# Google OAuth
Oauth__Google__ClientId=YOUR_GOOGLE_CLIENT_ID

# Facebook OAuth
Oauth__Facebook__AppId=YOUR_FACEBOOK_APP_ID
Oauth__Facebook__AppSecret=YOUR_FACEBOOK_APP_SECRET

# JWT Configuration
Jwt__SigningKey=your-long-secret-key-min-32-chars
Jwt__Issuer=goimon-api
Jwt__Audience=goimon-client
Jwt__ExpiryMinutes=1440

# OTP Configuration
Otp__ExpiryMinutes=10
Otp__MaxAttempts=3
Otp__EmailProvider=placeholder
Otp__SmsProvider=placeholder

# Database (if needed)
ConnectionStrings__DefaultConnection=your-database-connection-string
```

---

## Troubleshooting

### "Invalid Google ID token"
- Ensure the token has been freshly generated (within 1 hour)
- Verify the Client ID matches Google Cloud Console
- Check token is sent in the correct format

### "Invalid Facebook access token"
- Ensure access token is still valid (not expired)
- Verify App ID and permissions are correct
- Facebook tokens typically expire within 60 days for long-lived tokens

### "User already registered"
- User exists in the database with same email or OAuth ID
- Use login endpoint instead, or manually delete user from database

### OTP not working
- Current implementation uses placeholders - check server logs
- Integrate real email/SMS provider in OtpService.cs
- Verify OTP expires in 10 minutes and allows 3 failed attempts

### JWT Token validation fails
- Ensure signing key in config matches what was used to generate token
- Check token expiry (default 24 hours)
- Verify audience/issuer claims match

---

## Security Best Practices

1. **Environment Variables:** Never commit secrets to source control
2. **HTTPS Only:** Always use HTTPS in production
3. **Token Rotation:** Implement token refresh mechanisms for long sessions
4. **Rate Limiting:** Add rate limits to prevent brute force OTP attempts
5. **CORS Configuration:** Restrict CORS origins to trusted domains
6. **Database:** Use strong passwords and encryption for sensitive data
7. **Secret Rotation:** Regularly rotate signing keys and OAuth secrets
8. **Audit Logging:** Log all authentication attempts for security analysis

---

## Next Steps (Future Enhancements)

1. **Social Account Linking:** Allow users to link multiple OAuth providers
2. **Password Reset:** Implement password-based authentication alongside OAuth
3. **MFA:** Add multi-factor authentication (2FA) using TOTP or SMS
4. **Session Management:** Implement persistent sessions and logout functionality
5. **Refresh Tokens:** Add JWT refresh token mechanism for extended sessions
6. **OAuth Scope Management:** Request granular permissions from OAuth providers

---

**For questions or issues, file an issue in the GoiMon repository.**
