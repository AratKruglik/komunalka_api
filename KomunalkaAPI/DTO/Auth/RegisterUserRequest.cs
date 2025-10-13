using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

public class RegisterUserRequest
{
    [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ім'я користувача повинно бути від 3 до 100 символів")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    [StringLength(255, ErrorMessage = "Email занадто довгий")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Пароль обов'язковий")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Пароль повинен бути від 8 до 100 символів")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Пароль повинен містити принаймні одну велику літеру, одну маленьку літеру та одну цифру")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Підтвердження пароля обов'язкове")]
    [Compare("Password", ErrorMessage = "Паролі не співпадають")]
    public required string ConfirmPassword { get; set; }
}
