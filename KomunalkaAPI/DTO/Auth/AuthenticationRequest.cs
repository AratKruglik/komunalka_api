using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

public class AuthenticationRequest
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; set; }
}
