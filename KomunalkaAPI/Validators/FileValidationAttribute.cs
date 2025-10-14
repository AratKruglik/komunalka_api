using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace KomunalkaAPI.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class FileValidationAttribute : ValidationAttribute
{
    private readonly long _maxFileSizeInMB;
    private readonly string[] _allowedExtensions;

    public FileValidationAttribute()
    {
        // Default values (will be overridden from configuration)
        _maxFileSizeInMB = 10;
        _allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
        {
            return ValidationResult.Success;
        }

        // Get configuration from service provider
        var configuration = validationContext.GetService<IConfiguration>();
        var maxFileSizeInMB = configuration?.GetValue<int>("ImageSettings:MaxFileSizeInMB") ?? _maxFileSizeInMB;
        var allowedExtensions = configuration?.GetSection("ImageSettings:AllowedExtensions").Get<string[]>() ?? _allowedExtensions;

        var maxFileSizeInBytes = maxFileSizeInMB * 1024 * 1024;

        // Check file size
        if (file.Length > maxFileSizeInBytes)
        {
            return new ValidationResult($"File size must not exceed {maxFileSizeInMB} MB");
        }

        // Check file extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return new ValidationResult($"File type must be one of: {string.Join(", ", allowedExtensions)}");
        }

        // Check if file is actually an image by reading its header
        if (!IsValidImageFile(file))
        {
            return new ValidationResult("File is not a valid image");
        }

        return ValidationResult.Success;
    }

    private bool IsValidImageFile(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var buffer = new byte[8];
            stream.Read(buffer, 0, buffer.Length);

            // Check for JPEG signature (FF D8 FF)
            if (buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF)
                return true;

            // Check for PNG signature (89 50 4E 47)
            if (buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47)
                return true;

            // Check for WebP signature (52 49 46 46 ... 57 45 42 50)
            if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46)
            {
                stream.Seek(8, SeekOrigin.Begin);
                stream.Read(buffer, 0, 4);
                if (buffer[0] == 0x57 && buffer[1] == 0x45 && buffer[2] == 0x42 && buffer[3] == 0x50)
                    return true;
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
