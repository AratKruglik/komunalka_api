namespace KomunalkaAPI.DTO.Auth;

public class OAuthUserInfo
{
    public required string Provider { get; set; }
    public required string ExternalId { get; set; }
    public required string Email { get; set; }
    public bool EmailVerified { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Username { get; set; }
    public string? AvatarUrl { get; set; }
    public Dictionary<string, object>? AdditionalData { get; set; }
}
