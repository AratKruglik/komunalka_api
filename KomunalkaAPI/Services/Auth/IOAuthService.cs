using KomunalkaAPI.DTO.Auth;

namespace KomunalkaAPI.Services.Auth;

/// <summary>
/// OAuth authentication service interface
/// Handles OAuth login, linking, and unlinking
/// </summary>
public interface IOAuthService
{
    /// <summary>
    /// Authenticate user via OAuth provider using ID token
    /// </summary>
    /// <param name="provider">Provider name (Google, Apple, GitHub)</param>
    /// <param name="token">ID token or access token from provider</param>
    /// <returns>Authentication response with JWT tokens</returns>
    Task<AuthenticationResponse?> AuthenticateWithProviderAsync(string provider, string token);

    /// <summary>
    /// Handle OAuth callback (authorization code flow)
    /// </summary>
    /// <param name="request">Callback request with code and state</param>
    /// <returns>Authentication response with JWT tokens</returns>
    Task<AuthenticationResponse?> HandleCallbackAsync(OAuthCallbackRequest request);

    /// <summary>
    /// Get authorization URL for provider
    /// </summary>
    /// <param name="provider">Provider name</param>
    /// <param name="redirectUri">Optional redirect URI</param>
    /// <returns>Authorization URL</returns>
    string GetAuthorizationUrl(string provider, string? redirectUri = null);

    /// <summary>
    /// Link OAuth provider to existing user
    /// </summary>
    /// <param name="userId">User ID to link provider to</param>
    /// <param name="provider">Provider name</param>
    /// <param name="token">ID token from provider</param>
    /// <returns>True if linked successfully</returns>
    Task<bool> LinkProviderAsync(int userId, string provider, string token);

    /// <summary>
    /// Unlink OAuth provider from user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="provider">Provider name to unlink</param>
    /// <returns>True if unlinked successfully</returns>
    Task<bool> UnlinkProviderAsync(int userId, string provider);
}
