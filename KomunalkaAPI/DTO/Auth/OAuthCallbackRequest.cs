using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

/// <summary>
/// Request for OAuth callback (authorization code flow)
/// Used for web applications that redirect to provider
/// </summary>
public class OAuthCallbackRequest
{
    /// <summary>
    /// OAuth provider name
    /// </summary>
    [Required(ErrorMessage = "Provider is required")]
    public required string Provider { get; set; }

    /// <summary>
    /// Authorization code from OAuth provider
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// State parameter for CSRF protection
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Error code if authentication failed
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Error description if authentication failed
    /// </summary>
    public string? ErrorDescription { get; set; }
}
