using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Models;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(AuthProvider), nameof(ExternalId), IsUnique = true, Name = "IX_User_Provider_ExternalId")]
public class User
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Username is required")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
    public required string Username { get; set; }

    [StringLength(100, ErrorMessage = "First name is too long")]
    public string? FirstName { get; set; }

    [StringLength(100, ErrorMessage = "Last name is too long")]
    public string? LastName { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, ErrorMessage = "Phone number is too long")]
    public string? PhoneNumber { get; set; }

    [StringLength(500, ErrorMessage = "Password is too long")]
    public string? Password { get; set; } // Nullable for OAuth users

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email is too long")]
    public required string Email { get; set; }

    public List<UserAddress>? UserAddresses { get; set; }

    [StringLength(50, ErrorMessage = "Role is too long")]
    public string? Role { get; set; } = "User";

    // OAuth fields
    [StringLength(50, ErrorMessage = "Auth provider is too long")]
    public string? AuthProvider { get; set; } = "Local"; // "Local", "Google", "Apple", "GitHub"

    [StringLength(500, ErrorMessage = "External ID is too long")]
    public string? ExternalId { get; set; } // ID from OAuth provider

    public bool EmailVerified { get; set; } = false; // OAuth emails are auto-verified

    public DateTime? LastLoginAt { get; set; }

    [StringLength(500, ErrorMessage = "Avatar path is too long")]
    public string? AvatarOptimizedPath { get; set; }

    [StringLength(500, ErrorMessage = "Avatar thumbnail path is too long")]
    public string? AvatarThumbnailPath { get; set; }

    [StringLength(50, ErrorMessage = "Avatar mime type is too long")]
    public string? AvatarMimeType { get; set; }

    public long? AvatarSizeInBytes { get; set; }
    public int? AvatarWidth { get; set; }
    public int? AvatarHeight { get; set; }

    public List<RefreshToken>? RefreshTokens { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
