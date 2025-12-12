using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

/// <summary>
/// Request for OAuth login using ID token flow
/// Used for mobile apps and SPAs that authenticate with provider SDK
/// </summary>
public class OAuthLoginRequest
{
    /// <summary>
    /// OAuth provider name
    /// </summary>
    [Required(ErrorMessage = "Provider is required")]
    public required string Provider { get; set; }

    /// <summary>
    /// ID token or access token from OAuth provider
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    public required string Token { get; set; }
}
