using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using HotChocolate;
using HotChocolate.Types;
using GoiMon.Api.Features.Authentication.Dtos;
using GoiMon.Api.Infrastructure.Services;

namespace GoiMon.Api.Features.Authentication.Mutations;

[ExtendObjectType("Mutation")]
public class AuthenticationMutations
{
    private readonly ILogger<AuthenticationMutations> _logger;

    public AuthenticationMutations(ILogger<AuthenticationMutations> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Register a new user with Google OAuth.
    /// Generates OTP and sends it to the user's email/SMS.
    /// </summary>
    [GraphQLType(typeof(AuthenticationPayload))]
    public async Task<AuthenticationPayload> RegisterWithGoogleAsync(
        RegisterWithOAuthInput input,
        [Service] IOAuthExchangeService oauthService,
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] IOtpService otpService)
    {
        try
        {
            // Exchange Google token for user info
            var oauthUser = await oauthService.ExchangeGoogleTokenAsync(input.Token);

            using var context = contextFactory.CreateDbContext();

            // Check if user already exists by email or GoogleId
            var existingUser = context.Users.FirstOrDefault(u =>
                u.Email == oauthUser.Email || u.GoogleId == oauthUser.UserId);

            if (existingUser != null)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("User already registered with this email or Google account.")
                        .SetExtension("code", "USER_ALREADY_EXISTS")
                        .Build());
            }

            // Create new user (Assign to default pilot tenant for now)
            var userId = Guid.NewGuid();
            var pilotTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var user = User.CreateFromOAuth(
                id: userId,
                email: oauthUser.Email,
                firstName: oauthUser.FirstName,
                lastName: oauthUser.LastName,
                photoUrl: oauthUser.PhotoUrl,
                googleId: oauthUser.UserId,
                tenantId: pilotTenantId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Generate OTP
            _ = await otpService.GenerateOtpAsync(userId, input.OtpDeliveryMethod, contextFactory);

            _logger.LogInformation("New user registered with Google: {Email}", oauthUser.Email);

            return new AuthenticationPayload
            {
                User = UserDto.FromEntity(user),
                Token = null,
                RequiresOtpVerification = true,
                Message = $"Verification code sent to {input.OtpDeliveryMethod}. Please verify to complete registration."
            };
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google registration failed");
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Google registration failed. Please try again.")
                    .SetExtension("code", "GOOGLE_REGISTRATION_FAILED")
                    .Build());
        }
    }

    /// <summary>
    /// Register a new user with Facebook OAuth.
    /// Generates OTP and sends it to the user's email/SMS.
    /// </summary>
    [GraphQLType(typeof(AuthenticationPayload))]
    public async Task<AuthenticationPayload> RegisterWithFacebookAsync(
        RegisterWithOAuthInput input,
        [Service] IOAuthExchangeService oauthService,
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] IOtpService otpService)
    {
        try
        {
            // Exchange Facebook token for user info
            var oauthUser = await oauthService.ExchangeFacebookTokenAsync(input.Token);

            using var context = contextFactory.CreateDbContext();

            // Check if user already exists by email or FacebookId
            var existingUser = context.Users.FirstOrDefault(u =>
                u.Email == oauthUser.Email || u.FacebookId == oauthUser.UserId);

            if (existingUser != null)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("User already registered with this email or Facebook account.")
                        .SetExtension("code", "USER_ALREADY_EXISTS")
                        .Build());
            }

            // Create new user (Assign to default pilot tenant for now)
            var userId = Guid.NewGuid();
            var pilotTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var user = User.CreateFromOAuth(
                id: userId,
                email: oauthUser.Email,
                firstName: oauthUser.FirstName,
                lastName: oauthUser.LastName,
                photoUrl: oauthUser.PhotoUrl,
                facebookId: oauthUser.UserId,
                tenantId: pilotTenantId);

            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Generate OTP
            _ = await otpService.GenerateOtpAsync(userId, input.OtpDeliveryMethod, contextFactory);

            _logger.LogInformation("New user registered with Facebook: {Email}", oauthUser.Email);

            return new AuthenticationPayload
            {
                User = UserDto.FromEntity(user),
                Token = null,
                RequiresOtpVerification = true,
                Message = $"Verification code sent to {input.OtpDeliveryMethod}. Please verify to complete registration."
            };
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facebook registration failed");
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Facebook registration failed. Please try again.")
                    .SetExtension("code", "FACEBOOK_REGISTRATION_FAILED")
                    .Build());
        }
    }

    /// <summary>
    /// Login with Google OAuth.
    /// If user is not yet verified, returns OTP requirement.
    /// If user is verified, returns JWT token immediately.
    /// </summary>
    [GraphQLType(typeof(AuthenticationPayload))]
    public async Task<AuthenticationPayload> LoginWithGoogleAsync(
        LoginWithOAuthInput input,
        [Service] IOAuthExchangeService oauthService,
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] IOtpService otpService,
        [Service] IJwtTokenService jwtService)
    {
        try
        {
            // Exchange Google token for user info
            var oauthUser = await oauthService.ExchangeGoogleTokenAsync(input.Token);

            using var context = contextFactory.CreateDbContext();

            // Look up user by email or GoogleId
            var user = context.Users.FirstOrDefault(u =>
                u.Email == oauthUser.Email || u.GoogleId == oauthUser.UserId);

            if (user == null)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("User not found. Please register first.")
                        .SetExtension("code", "USER_NOT_FOUND")
                        .Build());
            }

            // If user is verified, issue JWT token immediately
            if (user.IsVerified)
            {
                var token = jwtService.GenerateToken(user.Id, user.TenantId, user.Email, user.Role.ToString(), true);
                _logger.LogInformation("User logged in with Google: {Email}", user.Email);

                return new AuthenticationPayload
                {
                    User = UserDto.FromEntity(user),
                    Token = token,
                    RequiresOtpVerification = false,
                    Message = "Login successful."
                };
            }

            // If not verified, generate new OTP
            _ = await otpService.GenerateOtpAsync(user.Id, input.OtpDeliveryMethod, contextFactory);

            _logger.LogInformation("Unverified user attempting login with Google: {Email}", user.Email);

            return new AuthenticationPayload
            {
                User = UserDto.FromEntity(user),
                Token = null,
                RequiresOtpVerification = true,
                Message = $"Account requires verification. Code sent to {input.OtpDeliveryMethod}."
            };
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google login failed");
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Google login failed. Please try again.")
                    .SetExtension("code", "GOOGLE_LOGIN_FAILED")
                    .Build());
        }
    }

    /// <summary>
    /// Login with Facebook OAuth.
    /// If user is not yet verified, returns OTP requirement.
    /// If user is verified, returns JWT token immediately.
    /// </summary>
    [GraphQLType(typeof(AuthenticationPayload))]
    public async Task<AuthenticationPayload> LoginWithFacebookAsync(
        LoginWithOAuthInput input,
        [Service] IOAuthExchangeService oauthService,
        [Service] IDbContextFactory<AppDbContext> contextFactory,
        [Service] IOtpService otpService,
        [Service] IJwtTokenService jwtService)
    {
        try
        {
            // Exchange Facebook token for user info
            var oauthUser = await oauthService.ExchangeFacebookTokenAsync(input.Token);

            using var context = contextFactory.CreateDbContext();

            // Look up user by email or FacebookId
            var user = context.Users.FirstOrDefault(u =>
                u.Email == oauthUser.Email || u.FacebookId == oauthUser.UserId);

            if (user == null)
            {
                throw new GraphQLException(
                    ErrorBuilder.New()
                        .SetMessage("User not found. Please register first.")
                        .SetExtension("code", "USER_NOT_FOUND")
                        .Build());
            }

            // If user is verified, issue JWT token immediately
            if (user.IsVerified)
            {
                var token = jwtService.GenerateToken(user.Id, user.TenantId, user.Email, user.Role.ToString(), true);
                _logger.LogInformation("User logged in with Facebook: {Email}", user.Email);

                return new AuthenticationPayload
                {
                    User = UserDto.FromEntity(user),
                    Token = token,
                    RequiresOtpVerification = false,
                    Message = "Login successful."
                };
            }

            // If not verified, generate new OTP
            _ = await otpService.GenerateOtpAsync(user.Id, input.OtpDeliveryMethod, contextFactory);

            _logger.LogInformation("Unverified user attempting login with Facebook: {Email}", user.Email);

            return new AuthenticationPayload
            {
                User = UserDto.FromEntity(user),
                Token = null,
                RequiresOtpVerification = true,
                Message = $"Account requires verification. Code sent to {input.OtpDeliveryMethod}."
            };
        }
        catch (GraphQLException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Facebook login failed");
            throw new GraphQLException(
                ErrorBuilder.New()
                    .SetMessage("Facebook login failed. Please try again.")
                    .SetExtension("code", "FACEBOOK_LOGIN_FAILED")
                    .Build());
        }
    }

    /// <summary>
    /// Verify OTP token and issue JWT if successful.
    /// </summary>
    [GraphQLType(typeof(OtpVerificationPayload))]
    public async Task<OtpVerificationPayload> VerifyOtpAsync(
        VerifyOtpInput input,
        [Service] IOtpService otpService,
        [Service] IJwtTokenService jwtService,
        [Service] IDbContextFactory<AppDbContext> contextFactory)
    {
        try
        {
            // Validate OTP
            var (isValid, message) = await otpService.ValidateOtpAsync(input.UserId, input.OtpToken, contextFactory);

            if (!isValid)
            {
                return new OtpVerificationPayload
                {
                    Success = false,
                    Token = null,
                    User = null,
                    Message = message
                };
            }

            using var context = contextFactory.CreateDbContext();

            // Load user and mark as verified
            var user = context.Users.Find(input.UserId);
            if (user == null)
            {
                return new OtpVerificationPayload
                {
                    Success = false,
                    Token = null,
                    User = null,
                    Message = "User not found."
                };
            }

            user.MarkAsVerified();
            context.Users.Update(user);
            await context.SaveChangesAsync();

            // Generate JWT token
            var token = jwtService.GenerateToken(user.Id, user.TenantId, user.Email, user.Role.ToString(), true);

            _logger.LogInformation("OTP verified for user: {Email}", user.Email);

            return new OtpVerificationPayload
            {
                Success = true,
                Token = token,
                User = UserDto.FromEntity(user),
                Message = "OTP verified successfully. You are now logged in."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OTP verification failed for user {UserId}", input.UserId);
            return new OtpVerificationPayload
            {
                Success = false,
                Token = null,
                User = null,
                Message = "OTP verification failed. Please try again."
            };
        }
    }
}
