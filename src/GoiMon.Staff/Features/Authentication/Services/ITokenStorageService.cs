namespace GoiMon.Staff.Features.Authentication.Services;

/// <summary>
/// Interface for JWT token storage in browser.
/// </summary>
public interface ITokenStorageService
{
    /// <summary>
    /// Stores JWT token in localStorage asynchronously.
    /// </summary>
    Task SetTokenAsync(string token);

    /// <summary>
    /// Retrieves JWT token from localStorage asynchronously.
    /// </summary>
    Task<string?> GetTokenAsync();

    /// <summary>
    /// Removes JWT token from localStorage asynchronously.
    /// </summary>
    Task ClearTokenAsync();

    /// <summary>
    /// Checks if token exists in localStorage asynchronously.
    /// </summary>
    Task<bool> HasTokenAsync();

    /// <summary>
    /// Stores JWT token in localStorage (synchronous wrapper).
    /// </summary>
    void SetToken(string token);

    /// <summary>
    /// Retrieves JWT token from localStorage (synchronous wrapper).
    /// </summary>
    string? GetToken();

    /// <summary>
    /// Removes JWT token from localStorage (synchronous wrapper).
    /// </summary>
    void ClearToken();

    /// <summary>
    /// Checks if token exists in localStorage (synchronous wrapper).
    /// </summary>
    bool HasToken();
}
