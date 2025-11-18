using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
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
            stream.ReadExactly(buffer, 0, buffer.Length);

            if (!TryReadHeader(stream, header))
            {
                return false;
            }

            if (IsJpeg(header))
                return true;

            // Check for WebP signature (52 49 46 46 ... 57 45 42 50)
            if (buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46)
            {
                stream.Seek(8, SeekOrigin.Begin);
                stream.ReadExactly(buffer, 0, 4);
                if (buffer[0] == 0x57 && buffer[1] == 0x45 && buffer[2] == 0x42 && buffer[3] == 0x50)
                    return true;
            }

            // Check for HEIC/HEIF signature
            // HEIC files start with: 00 00 00 [size] 66 74 79 70 (ftyp)
            // Followed by: 68 65 69 63 (heic) or 68 65 69 78 (heix) or 6D 69 66 31 (mif1)
            stream.Seek(0, SeekOrigin.Begin);
            stream.ReadExactly(buffer, 0, 12);

            if (IsHeic(header))
                return true;

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryReadHeader(Stream stream, Span<byte> header)
    {
        try
        {
            stream.ReadExactly(header);
            return true;
        }
        catch (EndOfStreamException)
        {
            return false;
        }
    }

    private static bool IsJpeg(ReadOnlySpan<byte> header)
        => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF;

    private static bool IsPng(ReadOnlySpan<byte> header)
        => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47;

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
