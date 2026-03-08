using System.Text;
using System.Text.Json;

namespace GoiMon.Portal.Features.Authentication;

/// <summary>
/// Lightweight GraphQL client for Portal auth (no StrawberryShake dependency).
/// </summary>
public class GraphQLAuthClient
{
    private readonly HttpClient _http;

    public GraphQLAuthClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<AuthPayload> AuthenticateWithGoogleAsync(string token, string otpDeliveryMethod)
    {
        var query = """
            mutation AuthenticateWithGoogle($token: String!, $otpDeliveryMethod: OtpDeliveryMethod!) {
              authenticateWithGoogle(input: { token: $token, provider: "Google", otpDeliveryMethod: $otpDeliveryMethod }) {
                user { id email firstName lastName isVerified }
                token
                requiresOtpVerification
                message
              }
            }
            """;

        var result = await ExecuteAsync<AuthenticateWithGoogleResponse>(query, new
        {
            token,
            otpDeliveryMethod = otpDeliveryMethod.ToUpperInvariant()
        });

        return result.AuthenticateWithGoogle;
    }

    public async Task<OtpPayload> VerifyOtpAsync(Guid userId, string otpToken)
    {
        var query = """
            mutation VerifyOtp($userId: UUID!, $otpToken: String!) {
              verifyOtp(input: { userId: $userId, otpToken: $otpToken }) {
                success
                token
                user { id email firstName lastName isVerified }
                message
              }
            }
            """;

        var result = await ExecuteAsync<VerifyOtpResponse>(query, new { userId, otpToken });
        return result.VerifyOtp;
    }

    public async Task<bool> ResendOtpAsync(Guid userId, string deliveryMethod)
    {
        var query = """
            mutation ResendOtp($userId: UUID!, $deliveryMethod: OtpDeliveryMethod!) {
              resendOtp(userId: $userId, deliveryMethod: $deliveryMethod)
            }
            """;

        var result = await ExecuteAsync<ResendOtpResponse>(query, new
        {
            userId,
            deliveryMethod = deliveryMethod.ToUpperInvariant()
        });
        return result.ResendOtp;
    }

    private async Task<T> ExecuteAsync<T>(string query, object variables)
    {
        var payload = JsonSerializer.Serialize(new { query, variables });
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _http.PostAsync("", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("errors", out var errors) && errors.GetArrayLength() > 0)
        {
            var msg = errors[0].GetProperty("message").GetString();
            throw new Exception(msg ?? "GraphQL error");
        }

        var data = doc.RootElement.GetProperty("data");
        return JsonSerializer.Deserialize<T>(data.GetRawText(), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }
}

// DTOs
public record AuthPayload
{
    public AuthUser User { get; init; } = default!;
    public string? Token { get; init; }
    public bool RequiresOtpVerification { get; init; }
    public string? Message { get; init; }
}

public record OtpPayload
{
    public bool Success { get; init; }
    public string? Token { get; init; }
    public AuthUser? User { get; init; }
    public string? Message { get; init; }
}

public record AuthUser
{
    public Guid Id { get; init; }
    public string Email { get; init; } = "";
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public bool IsVerified { get; init; }
}

file record AuthenticateWithGoogleResponse
{
    public AuthPayload AuthenticateWithGoogle { get; init; } = default!;
}

file record VerifyOtpResponse
{
    public OtpPayload VerifyOtp { get; init; } = default!;
}

file record ResendOtpResponse
{
    public bool ResendOtp { get; init; }
}
