using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace KomunalkaAPI.Validators;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
public class AvatarFileValidationAttribute : ValidationAttribute
{
    private readonly long _defaultMaxFileSizeInMb = 2;
    private readonly string[] _defaultAllowedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".heic", ".heif"];

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not IFormFile file)
        {
            return ValidationResult.Success;
        }

        var configuration = validationContext.GetService<IConfiguration>();
        var maxFileSizeInMb = configuration?.GetValue<int>("AvatarImageSettings:MaxFileSizeInMB") ?? (int)_defaultMaxFileSizeInMb;
        var allowedExtensions = configuration?.GetSection("AvatarImageSettings:AllowedExtensions").Get<string[]>()
                                ?? _defaultAllowedExtensions;

        var maxFileSizeInBytes = maxFileSizeInMb * 1024 * 1024;

        if (file.Length > maxFileSizeInBytes)
        {
            return new ValidationResult($"Avatar must not exceed {maxFileSizeInMb} MB");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
        {
            return new ValidationResult($"Avatar type must be one of: {string.Join(", ", allowedExtensions)}");
        }

        if (!IsValidImageFile(file))
        {
            return new ValidationResult("File is not a valid image");
        }

        return ValidationResult.Success;
    }

    private static bool IsValidImageFile(IFormFile file)
    {
        try
        {
            using var stream = file.OpenReadStream();
            Span<byte> buffer = stackalloc byte[12];

            stream.ReadExactly(buffer);

            return IsJpeg(buffer) || IsPng(buffer) || IsGif(buffer) || IsWebp(buffer) || IsHeic(buffer);
        }
        catch
        {
            return false;
        }
    }

    private static bool IsJpeg(ReadOnlySpan<byte> header)
        => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

    private static bool IsPng(ReadOnlySpan<byte> header)
        => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;

    private static bool IsGif(ReadOnlySpan<byte> header)
        => header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46;

    private static bool IsWebp(ReadOnlySpan<byte> header)
        => header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46 &&
           header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50;

    private static bool IsHeic(ReadOnlySpan<byte> header)
    {
        if (header[4] != 0x66 || header[5] != 0x74 || header[6] != 0x79 || header[7] != 0x70)
        {
            return false;
        }

        var brand = Encoding.ASCII.GetString(header[8..12]);
        return brand is "heic" or "heix" or "hevc" or "hevx" or "mif1" or "msf1";
    }
}
