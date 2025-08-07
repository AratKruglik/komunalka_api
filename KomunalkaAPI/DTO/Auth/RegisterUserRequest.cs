using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

public class RegisterUserRequest
{
    [Required]
    public required string Username { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }

    [Required]
    [Compare("Password")]
    public required string ConfirmPassword { get; set; }
}
