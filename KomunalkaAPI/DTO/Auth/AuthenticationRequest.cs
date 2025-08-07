using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

public class AuthenticationRequest
{
    [Required]
    public required string Email { get; set; }

    [Required]
    public required string Password { get; set; }
}
