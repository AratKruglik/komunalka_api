using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Models;

[Index(nameof(Email), IsUnique = true)]
public class User
{
    [Key]
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Password { get; set; }
    [EmailAddress]
    public required string Email { get; set; }
    public List<Address>? Addresses { get; set; }
    public string? Role { get; set; } = "User";
    public List<RefreshToken>? RefreshTokens { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
