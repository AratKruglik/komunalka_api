using System.ComponentModel.DataAnnotations;
using KomunalkaAPI.Validators;
using Microsoft.AspNetCore.Http;

namespace KomunalkaAPI.DTO;

public class UpdateUserRequest
{
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

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(255, ErrorMessage = "Email is too long")]
    public required string Email { get; set; }

    // Optional password change fields
    public string? CurrentPassword { get; set; }

    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit")]
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string? ConfirmNewPassword { get; set; }

    [AvatarFileValidation]
    public IFormFile? Avatar { get; set; }
}
