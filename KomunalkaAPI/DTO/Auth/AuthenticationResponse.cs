namespace KomunalkaAPI.DTO.Auth;

public class AuthenticationResponse
{
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? Expiration { get; set; }
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }

    /// <summary>
    /// Authentication provider (Local, Google, Apple, GitHub)
    /// </summary>
    public string? AuthProvider { get; set; }

    /// <summary>
    /// Whether the email has been verified
    /// </summary>
    public bool EmailVerified { get; set; }
}
