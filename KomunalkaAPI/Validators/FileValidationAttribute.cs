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
            var buffer = new byte[12];
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

            // Check for HEIC/HEIF signature
            // HEIC files start with: 00 00 00 [size] 66 74 79 70 (ftyp)
            // Followed by: 68 65 69 63 (heic) or 68 65 69 78 (heix) or 6D 69 66 31 (mif1)
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(buffer, 0, 12);

            if (buffer[4] == 0x66 && buffer[5] == 0x74 && buffer[6] == 0x79 && buffer[7] == 0x70)
            {
                // Check for heic, heix, hevc, hevx, mif1, msf1
                var brand = System.Text.Encoding.ASCII.GetString(buffer, 8, 4);
                if (brand == "heic" || brand == "heix" || brand == "hevc" ||
                    brand == "hevx" || brand == "mif1" || brand == "msf1")
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
