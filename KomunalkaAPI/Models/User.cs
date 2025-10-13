using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Models;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Ім'я користувача обов'язкове")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Ім'я користувача повинно бути від 3 до 100 символів")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Пароль обов'язковий")]
    [StringLength(500, ErrorMessage = "Пароль занадто довгий")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Email обов'язковий")]
    [EmailAddress(ErrorMessage = "Невірний формат email")]
    [StringLength(255, ErrorMessage = "Email занадто довгий")]
    public required string Email { get; set; }

    public List<Address>? Addresses { get; set; }

    [StringLength(50, ErrorMessage = "Роль занадто довга")]
    public string? Role { get; set; } = "User";

    public List<RefreshToken>? RefreshTokens { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
