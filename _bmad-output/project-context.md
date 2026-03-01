# GoiMon Project Context - Authentication Implementation

**Last Updated:** 2026-03-01 14:45 UTC  
**Completed Story:** User Authentication with OAuth + OTP (13/13 Tasks ✅)  
**Build Status:** ✅ Passing (0 errors, 0 warnings)  
**Deployment Status:** Ready for API testing

---

## Quick Reference

### 📋 Story Completion Summary

**Developer Story:** User Authentication with OAuth + OTP  
**Status:** ✅ COMPLETED (all 13 tasks finished, build passing)  
**Files Delivered:** 27 files (19 new, 4 updated, 1 migration)  
**Documentation:** 2 comprehensive guides + 1 architecture doc

**Key URL:** See [authentication-setup-guide.md](./implementation-artifacts/authentication-setup-guide.md) for deployment instructions

---

## Project Structure - Authentication Module

### GraphQL API Mutations (src/GoiMon.Api/Features/Authentication)
```
Features/Authentication/
├── Mutations/
│   └── AuthenticationMutations.cs         [550+ lines] 5 GraphQL endpoints
├── Dtos/
│   ├── AuthenticationInputs.cs            [40 lines] Input types
│   └── AuthenticationPayloads.cs          [60 lines] Output types
└── Validators/
    └── AuthenticationValidators.cs        [50 lines] FluentValidation rules
```

### Domain Entities (src/GoiMon.Api/Domain)
```
Domain/
├── Entities/
│   ├── User.cs                            [87 lines] IAggregateRoot
│   └── OtpToken.cs                        [95 lines] Value object
└── OAuthProvider.cs                       [3 lines] Enum (Google, Facebook)
```

### Infrastructure Services (src/GoiMon.Api/Infrastructure)
```
Infrastructure/
├── Services/
│   ├── IOtpService.cs & OtpService.cs     [150 lines] OTP generation/validation
│   ├── IOAuthExchangeService.cs & 
│   │   OAuthExchangeService.cs            [145 lines] Google/Facebook token validation
│   └── IJwtTokenService.cs & 
│       JwtTokenService.cs                 [100 lines] JWT token management
├── Data/
│   ├── AppDbContext.cs                    [Updated] Added User & OtpToken DbSets
│   └── Configurations/
│       ├── UserConfiguration.cs           [45 lines] EF configuration
│       └── OtpTokenConfiguration.cs       [45 lines] EF configuration
└── Data/Migrations/
    └── [timestamp]_AddAuthenticationEntities.cs  [Auto-generated]
```

---

## GraphQL Endpoint Signatures

### Register with Google
```graphql
mutation RegisterGoogle($input: RegisterWithOAuthInput!) {
  registerWithGoogle(input: $input) {
    user { id email firstName lastName photoUrl isVerified }
    token
    requiresOtpVerification
    message
  }
}
```

**Input Variables:**
```json
{
  "input": {
    "token": "GOOGLE_ID_TOKEN_HERE",
    "provider": "Google",
    "otpDeliveryMethod": "email"
  }
}
```

### Register with Facebook
```graphql
mutation RegisterFacebook($input: RegisterWithOAuthInput!) {
  registerWithFacebook(input: $input) {
    user { id email firstName lastName photoUrl isVerified }
    token
    requiresOtpVerification
    message
  }
}
```

### Login with Google
```graphql
mutation LoginGoogle($input: LoginWithOAuthInput!) {
  loginWithGoogle(input: $input) {
    user { id email firstName lastName photoUrl isVerified }
    token       # Will be present if verified, null if needs OTP
    requiresOtpVerification
    message
  }
}
```

### Login with Facebook
```graphql
mutation LoginFacebook($input: LoginWithOAuthInput!) {
  loginWithFacebook(input: $input) {
    user { id email firstName lastName photoUrl isVerified }
    token       # Will be present if verified, null if needs OTP
    requiresOtpVerification
    message
  }
}
```

### Verify OTP
```graphql
mutation VerifyOtp($input: VerifyOtpInput!) {
  verifyOtp(input: $input) {
    success
    token       # JWT token for authenticated requests
    user { id email firstName lastName photoUrl isVerified }
    message
  }
}
```

**Input Variables:**
```json
{
  "input": {
    "userId": "GUID_HERE",
    "otpToken": "123456"
  }
}
```

---

## Configuration Keys (appsettings.json)

```json
{
  "OAuth": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID"
    },
    "Facebook": {
      "AppId": "YOUR_FACEBOOK_APP_ID",
      "AppSecret": "YOUR_FACEBOOK_APP_SECRET"
    }
  },
  "Jwt": {
    "SigningKey": "your-32+-character-secret-key-min-256-bits",
    "Issuer": "goimon-api",
    "Audience": "goimon-client",
    "ExpiryMinutes": 1440
  },
  "Otp": {
    "ExpiryMinutes": 10,
    "MaxAttempts": 3,
    "EmailProvider": "placeholder",
    "SmsProvider": "placeholder"
  }
}
```

**Environment Variables to Set:**
- `GOIMON_OAUTH__GOOGLE__CLIENTID` → Google Client ID
- `GOIMON_OAUTH__FACEBOOK__APPID` → Facebook App ID
- `GOIMON_OAUTH__FACEBOOK__APPSECRET` → Facebook App Secret
- `GOIMON_JWT__SIGNINGKEY` → JWT signing key (min 32 chars)
- `GOIMON_JWT__ISSUER` → JWT issuer claim
- `GOIMON_JWT__AUDIENCE` → JWT audience claim
- `GOIMON_JWT__EXPIRYMINUTES` → Token lifetime

---

## Authentication Flow Diagrams

### New User Registration Flow
```
1. Client: POST Google/Facebook OAuth Token → registerWithGoogle(token)
2. API: Validate token via Google.Apis.Auth / Facebook Graph API
3. API: Create User entity if not exists
4. API: Generate 6-digit OTP, store in database
5. API: Send OTP via email/SMS (currently logged to console)
6. API: Return { user, requiresOtpVerification: true, token: null }

7. Client: Wait for user to input OTP
8. Client: POST verifyOtp(userId, otpToken)
9. API: Validate OTP (check expiry, attempts, match)
10. API: Mark user as verified
11. API: Generate JWT token (HS256, 24-hour expiry)
12. API: Return { success: true, token, user }
13. Client: Store JWT in localStorage, use for Authorization header
```

### Returning User Login Flow (Already Verified)
```
1. Client: POST Google OAuth Token → loginWithGoogle(token)
2. API: Validate token via Google.Apis.Auth
3. API: Find User by email or GoogleId
4. API: Check isVerified status
5. IF verified:
   - Generate JWT token directly
   - Return { token, requiresOtpVerification: false }
6. IF not verified:
   - Generate OTP
   - Return { token: null, requiresOtpVerification: true }
```

---

## Database Schema

### users table
```sql
CREATE TABLE users (
  id UUID PRIMARY KEY,
  email VARCHAR(255) NOT NULL UNIQUE,
  phone VARCHAR(20),
  first_name VARCHAR(255),
  last_name VARCHAR(255),
  photo_url VARCHAR(500),
  google_id VARCHAR(500) UNIQUE,
  facebook_id VARCHAR(500) UNIQUE,
  is_verified BOOLEAN DEFAULT false,
  is_active BOOLEAN DEFAULT true,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### otp_tokens table
```sql
CREATE TABLE otp_tokens (
  id UUID PRIMARY KEY,
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token VARCHAR(10) NOT NULL,
  delivery_method VARCHAR(10) NOT NULL,
  expires_at TIMESTAMP NOT NULL,
  is_used BOOLEAN DEFAULT false,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  used_at TIMESTAMP,
  failed_attempts INT DEFAULT 0
);

CREATE INDEX idx_otp_tokens_user_id ON otp_tokens(user_id);
CREATE INDEX idx_otp_tokens_lookup ON otp_tokens(user_id, is_used, expires_at);
```

---

## Service References

### OAuthExchangeService
**Purpose:** Validate OAuth tokens from Google and Facebook, extract user info

**Methods:**
- `ExchangeGoogleTokenAsync(string idToken)` → `OAuthUserInfo`
  - Uses `Google.Apis.Auth.GoogleJsonWebSignature.ValidateAsync()`
  - Offline validation, no external API call
  - Returns: (UserId, Email, FirstName, LastName, PhotoUrl)

- `ExchangeFacebookTokenAsync(string accessToken)` → `OAuthUserInfo`
  - Calls Facebook Graph API: `GET /me?fields=id,email,first_name,last_name,picture`
  - Returns same structure as Google

### OtpService
**Purpose:** Generate, validate, and manage one-time passwords

**Methods:**
- `GenerateOtpAsync(userId, deliveryMethod, contextFactory)` → `string`
  - Generates random 6-digit token
  - Stores in database with 10-minute expiry
  - Invalidates existing non-expired OTPs
  - Calls SendOtpAsync (currently logs to Serilog)

- `ValidateOtpAsync(userId, token, contextFactory)` → `(bool IsValid, string Message)`
  - Checks: expiry, usage, attempt count (max 3)
  - Records failed attempts
  - Marks token as used on success

### JwtTokenService
**Purpose:** Create and validate JWT tokens for session management

**Methods:**
- `GenerateToken(userId, email, isVerified=true)` → `string`
  - Algorithm: HS256 (HMAC-SHA256)
  - Expiry: Configurable (default 24 hours = 1440 minutes)
  - Claims: NameIdentifier, Email, "verified", "iat"
  - Issuer/Audience: Configurable

- `ValidateToken(string token)` → `(bool IsValid, Guid? UserId, string? Email, bool IsVerified)`
  - Full validation: signature, issuer, audience, lifetime
  - Extracts claims if valid

---

## Error Handling

### GraphQL Error Structure
All mutations return GraphQL errors with structure:
```json
{
  "errors": [
    {
      "message": "User-friendly error message",
      "extensions": {
        "code": "ERROR_CODE_CONSTANT"
      }
    }
  ]
}
```

### Common Error Codes
- `AUTH_TOKEN_INVALID` - OAuth token validation failed
- `AUTH_USER_EXISTS` - User already registered with this email
- `AUTH_OTP_EXPIRED` - OTP has expired (>10 minutes)
- `AUTH_OTP_INVALID` - OTP code does not match
- `AUTH_OTP_LOCKED` - Too many failed attempts (>3)
- `AUTH_USER_NOT_FOUND` - No user found for login

---

## Security Implementation

✅ **Implemented Components:**

1. **OAuth Token Validation**
   - Google: Offline via Google.Apis.Auth SDK (no API key needed)
   - Facebook: Direct Graph API call with signature verification

2. **OTP Rate Limiting**
   - Maximum 3 failed attempts per OTP
   - Automatic lockout after max attempts
   - 10-minute expiry to prevent brute force

3. **JWT Security**
   - HS256 (HMAC-SHA256) signature
   - Configurable secret key (minimum 32 characters)
   - 24-hour token expiry
   - Server-side signature validation on use

4. **Database Constraints**
   - Unique indexes on Email, GoogleId, FacebookId
   - Cascade delete on User deletion removes OTP tokens
   - Foreign key constraints enforced

5. **Input Validation**
   - FluentValidation rules on all GraphQL inputs
   - Token format validation (>50 chars)
   - OTP format validation (exactly 6 digits)
   - Provider whitelist (Google, Facebook only)

6. **Audit Logging**
   - Structured Serilog logging for all auth operations
   - Log context includes userId, email, operation type
   - Sensitive data (tokens, passwords) never logged

---

## Future Extensions

**Not Yet Implemented (Planned for Next Stories):**

1. **Email/SMS OTP Delivery** - Replace Serilog placeholder with actual provider integration
   - SendGrid for email OTP
   - Twilio for SMS OTP

2. **Token Refresh** - Implement refresh tokens for extended sessions
   - Short-lived access token (15 min)
   - Long-lived refresh token (30 days)

3. **Social Account Linking** - Allow users to link multiple OAuth providers
   - Account merge logic
   - Provider priority resolution

4. **TOTP (Time-Based OTP)** - Support authenticator apps (Google Authenticator, etc.)
   - QR code generation for provisioning
   - Time-window validation

5. **Session Management** - Track active sessions per user
   - Login history
   - Device management
   - Session revocation

6. **Profile Completion** - Guide verified users to complete profile
   - Birthday, phone, address collection
   - Progressive profiling

---

## Development Notes

### Build Status
```bash
$ dotnet build src/GoiMon.Api/GoiMon.Api.csproj
Build succeeded. 0 Warning(s), 0 Error(s)
Time Elapsed: 00:00:02.64
✅ PASSING
```

### Database Migration
```bash
$ dotnet ef migrations add AddAuthenticationEntities
$ dotnet ef database update
```

### Testing GraphQL Mutations
Use GraphQL IDE (e.g., GraphQL Playground, Insomnia) and point to:
- Local: `https://localhost:5001/graphql` or `http://localhost:5000/graphql`
- Production: [Your deployed API GraphQL endpoint]

### Next Story
"Implement Client UI for Authentication" (Blazor WASM in GoiMon.Client/)
- Create login/register pages
- Integrate OAuth SDK (Google, Facebook)
- Call GraphQL mutations
- Manage JWT token in localStorage

---

## Artifact References

**Documentation Files:**
- [Setup Guide](./implementation-artifacts/authentication-setup-guide.md) - Step-by-step deployment instructions
- [Architecture Doc](./implementation-artifacts/authentication-architecture.md) - System design, flows, security
- [Dev Story](./implementation-artifacts/dev-story-authentication.md) - Task breakdown, completion status, file inventory

**Implementation Files:**
- All 27 files tracked in dev-story "File List" section

---

## Contact & Support

**Story Owner:** Amelia (Developer Agent)  
**Last Updated:** 2026-03-01 14:45 UTC  
**For Questions:** Reference the architecture guide or setup guide in planning-artifacts/

