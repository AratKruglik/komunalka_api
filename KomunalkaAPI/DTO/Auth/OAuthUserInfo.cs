namespace KomunalkaAPI.DTO.Auth;

/// <summary>
/// Normalized user information from OAuth provider
/// Internal DTO used for processing OAuth responses
/// </summary>
public class OAuthUserInfo
{
    /// <summary>
    /// OAuth provider name (Google, Apple, GitHub)
    /// </summary>
    public required string Provider { get; set; }

    /// <summary>
    /// External user ID from provider (sub claim)
    /// </summary>
    public required string ExternalId { get; set; }

    /// <summary>
    /// User email from provider
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Whether the email is verified by the provider
    /// </summary>
    public bool EmailVerified { get; set; }

    /// <summary>
    /// User's first name (if available)
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name (if available)
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Username or display name (if available)
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Avatar/profile picture URL from provider
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Additional provider-specific data
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; set; }
}
