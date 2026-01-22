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
    public string? AuthProvider { get; set; }
    public bool EmailVerified { get; set; }
}
