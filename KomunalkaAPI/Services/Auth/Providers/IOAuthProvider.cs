using KomunalkaAPI.DTO.Auth;

namespace KomunalkaAPI.Services.Auth.Providers;

/// <summary>
/// Interface for OAuth provider implementations
/// Each provider (Google, Apple, GitHub) implements this interface
/// </summary>
public interface IOAuthProvider
{
    /// <summary>
    /// Provider name (Google, Apple, GitHub)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Validate ID token or access token and extract user info
    /// </summary>
    /// <param name="token">ID token or access token from provider</param>
    /// <returns>Normalized user info or null if validation fails</returns>
    Task<OAuthUserInfo?> ValidateTokenAsync(string token);

    /// <summary>
    /// Exchange authorization code for tokens and get user info
    /// Used in authorization code flow
    /// </summary>
    /// <param name="code">Authorization code from provider</param>
    /// <param name="redirectUri">Redirect URI used in authorization request</param>
    /// <returns>Normalized user info or null if exchange fails</returns>
    Task<OAuthUserInfo?> ExchangeCodeAsync(string code, string? redirectUri = null);

    /// <summary>
    /// Get authorization URL for OAuth flow
    /// </summary>
    /// <param name="state">State parameter for CSRF protection</param>
    /// <param name="redirectUri">Redirect URI for callback</param>
    /// <returns>Authorization URL</returns>
    string GetAuthorizationUrl(string state, string? redirectUri = null);
}
