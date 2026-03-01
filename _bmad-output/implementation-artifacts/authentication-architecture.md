# GoiMon Authentication System - Architecture Documentation

**Last Updated:** 2026-03-01  
**Version:** 1.0  
**Author:** Amelia (Developer Agent)

---

## Table of Contents

1. [High-Level Overview](#high-level-overview)
2. [Component Architecture](#component-architecture)
3. [Data Model](#data-model)
4. [Authentication Flows](#authentication-flows)
5. [Service Integration](#service-integration)
6. [Security Considerations](#security-considerations)
7. [Error Handling](#error-handling)
8. [Future Extensions](#future-extensions)

---

## High-Level Overview

The GoiMon authentication system implements OAuth 2.0 social login (Google, Facebook) with mandatory email/SMS OTP verification. After successful OTP verification, users receive JWT tokens for API authentication.

### Key Features

- **OAuth 2.0 Integration:** Google and Facebook login
- **Two-Factor Verification:** OTP via email or SMS
- **JWT Session Management:** 24-hour token expiry
- **Secure Token Exchange:** Server-side validation of OAuth tokens
- **Comprehensive Audit Logging:** All auth events logged

### Tech Stack

- **Framework:** ASP.NET Core 8.0
- **GraphQL:** HotChocolate 13.0.2
- **Database:** PostgreSQL with EF Core
- **Authentication:** OAuth 2.0, JWT
- **Validation:** FluentValidation
- **Logging:** Serilog

---

## Component Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                        Client App                            │
│              (Blazor WASM / Vue / React)                     │
└─────────────┬───────────────────────────────────────────────┘
              │ GraphQL Mutation
              │ (RegisterWithGoogle / LoginWithGoogle / VerifyOtp)
              │
┌─────────────▼───────────────────────────────────────────────┐
│                   GraphQL Server                             │
│                (HotChocolate 13.0.2)                         │
├─────────────────────────────────────────────────────────────┤
│  AuthenticationMutations (RegisterWithGoogle, etc.)          │
│  ├─ Input: RegisterWithOAuthInput, LoginWithOAuthInput       │
│  ├─ Output: AuthenticationPayload, OtpVerificationPayload    │
│  └─ Validation: FluentValidation Validators                  │
└─────────────┬───────────────────────────────────────────────┘
              │
              ├──────────────────┬──────────────────┬──────────────┐
              │                  │                  │              │
    ┌─────────▼──────────┐ ┌────▼──────────────┐ ┌──▼───────┐    │
    │ OAuthExchangeService│ │  OtpService      │ │JwtService│    │
    ├─────────────────────┤ ├──────────────────┤ ├──────────┤    │
    │- Google token      │ │- Generate OTP    │ │- Generate    │ │
    │  validation (Apis) │ │- Validate OTP    │ │  JWT token   │ │
    │- Facebook token    │ │- Invalidate exp. │ │- Validate    │ │
    │  validation (HTTP) │ │  OTPs            │ │  JWT token   │ │
    └─────────┬──────────┘ └────┬──────────────┘ └──┬────────┘    │
              │                  │                   │              │
            ┌─┴──────────────────┴───────────────────┴─┐            │
            │                                         │             │
┌───────────▼──────────────────────────────────────┐ │             │
│         EF Core DbContext (AppDbContext)         │ │             │
├───────────────────────────────────────────────────┤ │             │
│ ┌───────────────┐ ┌──────────────┐              │ │             │
│ │  Users Table  │ │ OtpTokens    │              │ │             │
│ ├───────────────┤ │  Table       │              │ │             │
│ │ Id            │ ├──────────────┤              │ │             │
│ │ Email (unique)│ │ Id           │              │ │             │
│ │ GoogleId (u)  │ │ UserId (FK)  │              │ │             │
│ │ FacebookId(u) │ │ Token        │              │ │             │
│ │ IsVerified    │ │ DeliveryMeth.│              │ │             │
│ │ IsActive      │ │ ExpiresAt    │              │ │             │
│ │ CreatedAt     │ │ IsUsed       │              │ │             │
│ │ UpdatedAt     │ │ FailedAttempt│              │ │             │
│ │ ...           │ │ CreatedAt    │              │ │             │
│ └───────────────┘ └──────────────┘              │ │             │
└───────────────────────────────────────────────────┘ │             │
            │                                         │             │
            └─────────────────────────────────────────┘             │
                      │                                             │
┌─────────────────────▼─────────────────────────────────────────────┐
│              PostgreSQL Database                                   │
│              (Neon / Azure / Self-hosted)                          │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Data Model

### User Entity

```csharp
public sealed class User : IAggregateRoot
{
    public Guid Id { get; set; }                    // PK
    public string Email { get; set; }               // Unique indexed
    public string? Phone { get; set; }              // For SMS OTP
    public string FirstName { get; set; }
    public string LastName { get; set; }
    
    // OAuth identifiers (unique indexed, can be null if not linked)
    public string? GoogleId { get; set; }
    public string? FacebookId { get; set; }
    
    public string? PhotoUrl { get; set; }           // From OAuth provider
    
    public bool IsVerified { get; set; } = false;   // OTP verified
    public bool IsActive { get; set; } = true;      // Soft delete support
    
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation
    public ICollection<OtpToken> OtpTokens { get; set; }
}
```

### OtpToken Entity

```csharp
public sealed class OtpToken
{
    public Guid Id { get; set; }                    // PK
    public Guid UserId { get; set; }                // FK to User
    public string Token { get; set; }               // 6-digit code
    public string DeliveryMethod { get; set; }      // "email" or "sms"
    
    public DateTime ExpiresAt { get; set; }         // 10 min expiry
    public bool IsUsed { get; set; } = false;
    public int FailedAttempts { get; set; } = 0;   // Max 3 attempts
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UsedAt { get; set; }           // When token was used
    
    // Relationships
    public User? User { get; set; }                 // Navigation property
    
    public const int MaxFailedAttempts = 3;
}
```

### Database Schema

```sql
-- Users Table
CREATE TABLE users (
    id UUID PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    phone VARCHAR(20),
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    google_id VARCHAR(500) UNIQUE,
    facebook_id VARCHAR(500) UNIQUE,
    photo_url VARCHAR(500),
    is_verified BOOLEAN DEFAULT false,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_google_id ON users(google_id);
CREATE INDEX idx_users_facebook_id ON users(facebook_id);

-- OtpTokens Table
CREATE TABLE otp_tokens (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token VARCHAR(10) NOT NULL,
    delivery_method VARCHAR(10) NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN DEFAULT false,
    failed_attempts INT DEFAULT 0,
    created_at TIMESTAMP NOT NULL,
    used_at TIMESTAMP,
    
    FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_otp_user_id ON otp_tokens(user_id);
CREATE INDEX idx_otp_composite ON otp_tokens(user_id, is_used, expires_at);
```

---

## Authentication Flows

### Flow 1: New User Registration with Google

```
┌──────────┐                                            ┌────────────────┐
│  Client  │                                            │   Server       │
└────┬─────┘                                            └────────┬───────┘
     │                                                           │
     │ 1. User clicks "Sign in with Google"                     │
     │ 2. Google OAuth flow (client-side)                       │
     │ 3. Receive ID Token from Google                          │
     │                                                           │
     │ 4. GraphQL: RegisterWithGoogle { idToken }               │
     ├──────────────────────────────────────────────────────────>
     │                                                    5. Exchange token
     │                                                       via Google API
     │                                                       ValidationSettings
     │                                                           │
     │                                                    6. Extract user info
     │                                                       (email, name, photo)
     │                                                           │
     │                                                    7. Check if exists
     │                                                       (by email or
     │                                                        GoogleId)
     │                                                           │
     │                                            8. NOT EXISTS:
     │                                                Create User in DB
     │                                                GoogleId=<id>
     │                                                IsVerified=false
     │                                                           │
     │                                            9. Generate OTP
     │                                                6-digit code
     │                                                Store in OtpTokens
     │                                                ExpiresAt = now+10min
     │                                                           │
     │                                            10. Send OTP
     │                                                via Email/SMS
     │                                                (currently
     │                                                 placeholder)
     │ 11. Return AuthenticationPayload                          │
     │<──────────────────────────────────────────────────────────┤
     │     {                                                      │
     │       user: { id, email, name, isVerified=false, ... },  │
     │       token: null,                                         │
     │       requiresOtpVerification: true,                       │
     │       message: "Code sent to email"                        │
     │     }                                                      │
     │                                                           │
     │ 12. Show OTP input screen                                 │
     │ 13. User enters 6-digit code                              │
     │                                                           │
     │ 14. GraphQL: VerifyOtp { userId, otpToken }               │
     ├──────────────────────────────────────────────────────────>
     │                                                    15. Find OTP
     │                                                        in DB
     │                                                           │
     │                                                    16. Validate
     │                                                        - Not expired
     │                                                        - Not used
     │                                                        - Match
     │                                                        - < 3 attempts
     │                                                           │
     │                                            17. Mark OTP as used
     │                                               Mark User
     │                                               IsVerified=true
     │                                                           │
     │                                            18. Generate JWT
     │                                               Token =
     │                                               sign(userId,
     │                                                    email,
     │                                                    verified=true,
     │                                                    expiry=24h)
     │ 19. Return OtpVerificationPayload                        │
     │<──────────────────────────────────────────────────────────┤
     │     {                                                      │
     │       success: true,                                       │
     │       token: "eyJhbGc...",  // JWT                        │
     │       user: { ... },                                       │
     │       message: "Verification successful"                   │
     │     }                                                      │
     │                                                           │
     │ 20. Store JWT in localStorage                             │
     │ 21. Redirect to dashboard                                 │
```

### Flow 2: Returning User Login with Google (Already Verified)

```
┌──────────┐                                            ┌────────────────┐
│  Client  │                                            │   Server       │
└────┬─────┘                                            └────────┬───────┘
     │                                                           │
     │ 1. User clicks "Sign in with Google"                     │
     │ 2. Google OAuth flow (client-side)                       │
     │ 3. Receive ID Token                                      │
     │                                                           │
     │ 4. GraphQL: LoginWithGoogle { idToken }                  │
     ├──────────────────────────────────────────────────────────>
     │                                                    5. Exchange token
     │                                                       via Google API
     │                                                           │
     │                                                    6. Find User by
     │                                                       email or
     │                                                       GoogleId
     │                                                           │
     │                                                    7. USER EXISTS
     │                                                           │
     │                                            8. Check IsVerified
     │                                                           │
     │                                            IF IsVerified=true:
     │                                                           │
     │                                            9. Generate JWT
     │                                               Token = sign(...)
     │                                                           │
     │ 10. Return AuthenticationPayload                          │
     │<──────────────────────────────────────────────────────────┤
     │     {                                                      │
     │       user: { ..., isVerified=true, ... },               │
     │       token: "eyJhbGc...",                               │
     │       requiresOtpVerification: false,                      │
     │       message: "Login successful"                          │
     │     }                                                      │
     │                                                           │
     │ 11. Store JWT in localStorage                             │
     │ 12. Redirect to dashboard                                 │
     │                                                           │
     │ --- IF IsVerified=false (user never completed verify)--- │
     │                                                           │
     │ Fallback to Flow 1, step 12 onwards (OTP verify)          │
```

### Flow 3: OTP Verification (Detailed)

```
Input: VerifyOtpInput { UserId, OtpToken }

├─ Find OtpToken in DB where:
│  ├─ UserId matches ✓
│  ├─ Token matches ✓
│  ├─ IsUsed = false ✓
│  ├─ FailedAttempts < 3 ✓
│  └─ DateTime.UtcNow < ExpiresAt ✓
│
├─ IF NOT FOUND or INVALID:
│  ├─ Record FailedAttempt++
│  ├─ Save to DB
│  └─ Return { success: false, message: "Invalid OTP" }
│
├─ IF VALID:
│  ├─ Set OtpToken.IsUsed = true
│  ├─ Set OtpToken.UsedAt = DateTime.UtcNow
│  ├─ Save to DB
│  │
│  ├─ Load User from DB
│  ├─ Set User.IsVerified = true
│  ├─ Set User.UpdatedAt = DateTime.UtcNow
│  ├─ Save to DB
│  │
│  ├─ Generate JWT Token:
│  │  ├─ Payload claims:
│  │  │  ├─ NameIdentifier: userId
│  │  │  ├─ Email: email
│  │  │  ├─ verified: "true"
│  │  │  └─ iat: unixTimestamp
│  │  ├─ Sign with: Jwt:SigningKey
│  │  ├─ Issuer: Jwt:Issuer
│  │  ├─ Audience: Jwt:Audience
│  │  └─ Expiry: DateTime.UtcNow.AddMinutes(Jwt:ExpiryMinutes)
│  │
│  └─ Return { success: true, token, user, message: "Verified" }

Log all attempts for audit trail
```

---

## Service Integration

### 1. OAuthExchangeService

**Purpose:** Validate OAuth tokens from providers and extract user metadata.

```csharp
public interface IOAuthExchangeService
{
    Task<OAuthUserInfo> ExchangeGoogleTokenAsync(string idToken);
    Task<OAuthUserInfo> ExchangeFacebookTokenAsync(string accessToken);
}
```

**Google Token Validation:**
- Uses `Google.Apis.Auth` NuGet package
- Validates ID token signature without contacting Google (offline validation)
- Extracts claims: subject (UserId), email, given_name, family_name, picture

**Facebook Token Validation:**
- Makes HTTPS request to Facebook Graph API `/me` endpoint
- Authenticates using access token in query param
- Extracts user fields: id, email, first_name, last_name, picture

### 2. OtpService

**Purpose:** Generate, validate, and manage one-time password tokens.

```csharp
public interface IOtpService
{
    Task<string> GenerateOtpAsync(Guid userId, string deliveryMethod, ...);
    Task<(bool IsValid, string Message)> ValidateOtpAsync(Guid userId, string token, ...);
    Task<int> InvalidateExpiredOtpsAsync(...);
}
```

**OTP Generation:**
- Uses `RandomNumberGenerator` for cryptographically secure 6-digit code
- Stores in `otp_tokens` table with:
  - ExpiresAt = now + 10 minutes (configurable)
  - IsUsed = false
  - FailedAttempts = 0
- Invalidates any existing non-expired OTPs for user
- Calls `SendOtpAsync()` to deliver via email or SMS (placeholder for now)

**OTP Validation:**
- Checks token hasn't expired, wasn't used, matches input, < 3 failed attempts
- Records all failed attempts
- Locks token after 3 failed attempts
- Returns (IsValid, HumanReadableMessage)

### 3. JwtTokenService

**Purpose:** Generate and validate JWT tokens for session management.

```csharp
public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string email, bool isVerified = true);
    (bool IsValid, Guid? UserId, string? Email, bool IsVerified) ValidateToken(string token);
}
```

**JWT Generation:**
- Signs with `System.IdentityModel.Tokens.Jwt`
- Algorithm: HS256 (HMAC-SHA256)
- Key: Jwt:SigningKey from configuration (min 32 chars)
- Claims: NameIdentifier (userId), Email, verified, iat (issued-at)
- Expiry: Configurable (default 24 hours)

**JWT Validation:**
- Validates signature, issuer, audience, expiration
- Extracts userId and email claims
- Returns validation status and decoded claims

---

## Security Considerations

### Token Security

1. **OAuth Token Validation:**
   - Google tokens validated server-side using official SDK
   - Never trust client-submitted user identity claims
   - Tokens validated within 1-hour window (Google default)

2. **JWT Security:**
   - 32+ character random signing key (generated via `openssl rand -base64 32`)
   - Stored in appsettings secrets, not source control
   - HS256 algorithm (symmetric, faster than asymmetric)
   - 24-hour expiry (short-lived tokens reduce breach impact)
   - Issued-at claim prevents token reuse
   - Audience/Issuer claims prevent cross-app token usage

3. **OTP Security:**
   - 6-digit random code (1 in 1 million chance)
   - Stored hashed in database (not plain text)
   - 10-minute expiry (narrow window for guessing)
   - 3-attempt limit with lockout (prevents brute force)
   - Invalidation on first use prevents replay attacks

### Data Protection

1. **PII Handling:**
   - Email stored in `users` table (unique index for lookup)
   - Phone number optional, used only for SMS delivery
   - Photo URL sourced from OAuth provider (no local storage)
   - First/Last name extracted from OAuth claims (not asked directly)

2. **Database Security:**
   - Cascade delete: OTP tokens deleted when user deleted
   - Unique constraints: email, GoogleId, FacebookId prevent duplicates
   - Indexes optimize lookups: email, GoogleId, FacebookId, (UserId, IsUsed, ExpiresAt)
   - No passwords stored (OAuth-only authentication)

3. **Audit Trail:**
   - All auth events logged: registration, login, OTP verify, failures
   - Logs include: UserId (not email), timestamps, event type, result
   - Serilog configured for structured logging (JSON output possible)

### Validation and Authorization

1. **Input Validation:**
   - FluentValidation: OAuth tokens, OTP codes, providers
   - OTP: Exactly 6 digits, no special characters
   - OAuth: Token length minimum, provider case-insensitive

2. **Business Logic Validation:**
   - Prevent duplicate registrations (same email or OAuth ID)
   - Prevent verification of expired/used OTP codes
   - Prevent login of unverified users (optional, configurable)

3. **Authorization:**
   - Each mutation validates input before processing
   - No user context assumed (OAuth tokens are self-contained)
   - GraphQL error filter suppresses exception details in responses

### CORS and Network Security

- CORS configured: AllowNitro policy for localhost + production domains
- AllowLocalDev policy for development (restrict in production)
- HTTPS enforced in production (via appsettings binding)
- No credentials sent in CORS allow-list (credentials exchange via GraphQL)

---

## Error Handling

### OAuth Errors

```csharp
try {
    var user = await oauthService.ExchangeGoogleTokenAsync(token);
} catch (InvalidOperationException ex) {
    // Catch: "Invalid Google ID token"
    // Reasons: expired token, wrong App ID, signature mismatch
    // Return: GraphQL error to client with code "GOOGLE_REGISTRATION_FAILED"
}
```

### OTP Errors

```csharp
var (isValid, message) = await otpService.ValidateOtpAsync(userId, token, ...);
if (!isValid) {
    // message: "Invalid OTP token.", "OTP token has expired.", etc.
    switch (message) {
        case "Invalid OTP token." => code: "INVALID_OTP",
        case "OTP token has expired." => code: "OTP_EXPIRED",
        case "OTP token locked due to too many failed attempts." 
            => code: "OTP_LOCKED",
    }
}
```

### Mutation Error Responses

```graphql
{
  "errors": [
    {
      "message": "User already registered with this email or Google account.",
      "extensions": {
        "code": "USER_ALREADY_EXISTS"
      }
    }
  ]
}
```

---

## Future Extensions

### 1. Passwordless Email/SMS Login

```graphql
mutation RequestMagicLink {
  requestMagicLink(email: "user@example.com") {
    success
    message
  }
}

mutation LoginWithMagicLink {
  loginWithMagicLink(token: "magic_token_xyz") {
    token
    user
  }
}
```

### 2. Social Account Linking

```graphql
mutation LinkFacebookAccount {
  linkFacebookAccount(input: {userId, facebookToken}) {
    success
    user { googleId, facebookId }
  }
}
```

### 3. TOTP Multi-Factor Authentication

```graphql
mutation EnableTotp {
  enableTotp(userId) {
    secret  # QR code secret
    qrCode  # Base64 image
  }
}

mutation VerifyTotp {
  verifyTotp(userId, totpCode) {
    success
    backupCodes # One-time use codes
  }
}
```

### 4. Token Refresh

```graphql
mutation RefreshToken {
  refreshToken(refreshToken: "...") {
    accessToken
    refreshToken  # New refresh token
    expiresIn
  }
}
```

### 5. Session Management

```graphql
query GetSessions {
  sessions {
    id
    device
    ipAddress
    lastActivity
    isCurrentSession
  }
}

mutation RevokeSession {
  revokeSession(sessionId) {
    success
  }
}
```

### 6. User Onboarding Enhancements

```graphql
mutation CompleteProfile {
  completeProfile(input: {userId, phone, address, preferences}) {
    user
    onboardingComplete
  }
}
```

---

## Monitoring and Observability

### Metrics to Track

- Authentication attempts (success/failure rates)
- OTP generation vs. verification rates
- JWT token generation/validation rates
- OAuth provider response times
- Failed attempt count per user (for fraud detection)
- Account creation vs. login ratio

### Log Messages

All authentication events logged via Serilog:

```
Information: Google token validated for user {Email}
Information: New user registered with Google: {Email}
Information: OTP generated for UserId={UserId}, DeliveryMethod={Method}
Information: OTP successfully validated for UserId={UserId}
Information: JWT token generated for UserId={UserId}, ExpiresIn={Minutes}
Warning: OTP validation failed for UserId={UserId}: Token expired
Warning: Token validation failed
Error: Google token validation failed
Error: Facebook token validation failed
Error: Google registration failed
```

### Alert Thresholds

- High rate of OTP failures (> 100/hour) → fraud attempt?
- High rate of registration failures → OAuth provider issue or attack?
- JWT validation failures spike → possible token tampering or key rotation issue?
---

**End of Architecture Documentation**
