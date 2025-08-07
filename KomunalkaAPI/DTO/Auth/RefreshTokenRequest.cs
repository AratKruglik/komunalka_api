using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

public class RefreshTokenRequest
{
    [Required]
    public required string RefreshToken { get; set; }
}
