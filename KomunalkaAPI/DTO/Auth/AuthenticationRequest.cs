using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

public class AuthenticationRequest
{
    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Пароль обов'язковий")]
    public required string Password { get; set; }
}
