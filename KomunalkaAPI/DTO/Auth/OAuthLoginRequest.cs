using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

public class OAuthLoginRequest
{
    [Required(ErrorMessage = "Provider is required")]
    public required string Provider { get; set; }

    [Required(ErrorMessage = "Token is required")]
    public required string Token { get; set; }
}
