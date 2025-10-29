using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Models;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    public required string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(500, ErrorMessage = "Password is too long")]
    public required string Password { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email is too long")]
    public required string Email { get; set; }

    public List<UserAddress>? UserAddresses { get; set; }

    [StringLength(50, ErrorMessage = "Role is too long")]
    public string? Role { get; set; } = "User";

    public List<RefreshToken>? RefreshTokens { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
