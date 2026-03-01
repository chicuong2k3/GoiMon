using Blazored.LocalStorage;

namespace GoiMon.Client.Features.Authentication.Services;

/// <summary>
/// Service for managing JWT token storage in browser localStorage.
/// </summary>
public class TokenStorageService : ITokenStorageService
{
    private const string TokenKey = "auth_token";
    private readonly ILocalStorageService _localStorage;

    public TokenStorageService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    /// <inheritdoc />
    public async Task SetTokenAsync(string token)
    {
        await _localStorage.SetItemAsStringAsync(TokenKey, token);
    }

    /// <inheritdoc />
    public async Task<string?> GetTokenAsync()
    {
        try
        {
            return await _localStorage.GetItemAsStringAsync(TokenKey);
        }
        catch
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task ClearTokenAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
    }

    /// <inheritdoc />
    public async Task<bool> HasTokenAsync()
    {
        var token = await GetTokenAsync();
        return !string.IsNullOrEmpty(token);
    }

    // Synchronous versions for compatibility (blocking - use async versions where possible)
    public void SetToken(string token)
    {
        SetTokenAsync(token).GetAwaiter().GetResult();
    }

    public string? GetToken()
    {
        return GetTokenAsync().GetAwaiter().GetResult();
    }

    public void ClearToken()
    {
        ClearTokenAsync().GetAwaiter().GetResult();
    }

    public bool HasToken()
    {
        return HasTokenAsync().GetAwaiter().GetResult();
    }
}
