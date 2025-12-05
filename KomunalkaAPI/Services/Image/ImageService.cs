using KomunalkaAPI.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using ImageMagick;

namespace KomunalkaAPI.Services.Image;

public class ImageService : IImageService
{
    private readonly IFileStorageService _fileStorage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageService> _logger;
    private readonly int _optimizedWidth;
    private readonly int _thumbnailWidth;
    private readonly int _avatarOptimizedWidth;
    private readonly int _avatarThumbnailWidth;
    private readonly int _jpegQuality;

    public ImageService(
        IFileStorageService fileStorage,
        IConfiguration configuration,
        ILogger<ImageService> logger)
    {
        _fileStorage = fileStorage;
        _configuration = configuration;
        _logger = logger;

        _optimizedWidth = _configuration.GetValue<int>("ImageSettings:OptimizedImageWidth", 800);
        _thumbnailWidth = _configuration.GetValue<int>("ImageSettings:ThumbnailWidth", 200);
        _jpegQuality = _configuration.GetValue<int>("ImageSettings:JpegQuality", 85);
        _avatarOptimizedWidth = _configuration.GetValue<int>("AvatarImageSettings:OptimizedImageWidth", _optimizedWidth);
        _avatarThumbnailWidth = _configuration.GetValue<int>("AvatarImageSettings:ThumbnailWidth", _thumbnailWidth);
    }

    public async Task<(string optimizedPath, string thumbnailPath, long optimizedSize, long thumbnailSize, int width, int height)>
        ProcessImageAsync(Stream imageStream, string fileName, int readingId)
    {
        var subfolder = $"{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month:D2}/{readingId}";
        return await ProcessImageInternalAsync(
            imageStream,
            fileName,
            subfolder,
            _optimizedWidth,
            _thumbnailWidth,
            StorageScope.MeterReading);
    }

    public async Task<(string optimizedPath, string thumbnailPath, long optimizedSize, long thumbnailSize, int width, int height)>
        ProcessAvatarImageAsync(Stream imageStream, string fileName, int userId)
    {
        var subfolder = $"avatars/{userId}/{DateTime.UtcNow:yyyy/MM}";
        return await ProcessImageInternalAsync(
            imageStream,
            fileName,
            subfolder,
            _avatarOptimizedWidth,
            _avatarThumbnailWidth,
            StorageScope.Avatar);
    }

    private async Task<(string optimizedPath, string thumbnailPath, long optimizedSize, long thumbnailSize, int width, int height)>
        ProcessImageInternalAsync(Stream imageStream, string fileName, string subfolder, int optimizedWidth, int thumbnailWidth, StorageScope scope)
    {
        try
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            // Check if it's HEIC/HEIF - convert to JPEG first using ImageMagick
            if (extension == ".heic" || extension == ".heif")
            {
                return await ProcessHeicImageAsync(imageStream, fileName, subfolder, optimizedWidth, thumbnailWidth, scope);
            }

            // Process regular images with ImageSharp
            using var image = await SixLabors.ImageSharp.Image.LoadAsync(imageStream);

            // Auto-orient based on EXIF data
            image.Mutate(x => x.AutoOrient());

            // Remove EXIF metadata for privacy
            image.Metadata.ExifProfile = null;

            var originalWidth = image.Width;
            var originalHeight = image.Height;

            // Generate unique file names
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var baseFileName = Path.GetFileNameWithoutExtension(fileName);

            // Process optimized image
            var optimizedFileName = $"{baseFileName}_{timestamp}_optimized.jpg";
            var (optimizedPath, optimizedSize) = await SaveResizedImageAsync(
                image, subfolder, optimizedFileName, optimizedWidth, scope);

            // Process thumbnail
            var thumbnailFileName = $"{baseFileName}_{timestamp}_thumbnail.jpg";
            var (thumbnailPath, thumbnailSize) = await SaveResizedImageAsync(
                image, subfolder, thumbnailFileName, thumbnailWidth, scope);

            return (optimizedPath, thumbnailPath, optimizedSize, thumbnailSize, originalWidth, originalHeight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process image {FileName}", fileName);
            throw;
        }
    }

    private async Task<(string optimizedPath, string thumbnailPath, long optimizedSize, long thumbnailSize, int width, int height)>
        ProcessHeicImageAsync(Stream heicStream, string fileName, string subfolder, int optimizedWidth, int thumbnailWidth, StorageScope scope)
    {
        try
        {
            _logger.LogInformation("Processing HEIC/HEIF image: {FileName}", fileName);

            // Load HEIC with ImageMagick
            using var magickImage = new MagickImage(heicStream);

            // Auto-orient based on EXIF
            magickImage.AutoOrient();

            // Remove EXIF metadata for privacy
            magickImage.RemoveProfile("exif");
            magickImage.RemoveProfile("iptc");
            magickImage.RemoveProfile("xmp");

            var originalWidth = (int)magickImage.Width;
            var originalHeight = (int)magickImage.Height;

            // Generate unique file names
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var baseFileName = Path.GetFileNameWithoutExtension(fileName);

            // Process optimized image
            var optimizedFileName = $"{baseFileName}_{timestamp}_optimized.jpg";
            var (optimizedPath, optimizedSize) = await SaveResizedHeicImageAsync(
                magickImage, subfolder, optimizedFileName, optimizedWidth, scope);

            // Process thumbnail
            var thumbnailFileName = $"{baseFileName}_{timestamp}_thumbnail.jpg";
            var (thumbnailPath, thumbnailSize) = await SaveResizedHeicImageAsync(
                magickImage, subfolder, thumbnailFileName, thumbnailWidth, scope);

            return (optimizedPath, thumbnailPath, optimizedSize, thumbnailSize, originalWidth, originalHeight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process HEIC image {FileName}", fileName);
            throw;
        }
    }

    private async Task<(string path, long size)> SaveResizedHeicImageAsync(
        MagickImage sourceImage, string subfolder, string fileName, int maxWidth, StorageScope scope)
    {
        using var resizedImage = sourceImage.Clone();

        if (resizedImage.Width > maxWidth)
        {
            var ratio = (double)maxWidth / resizedImage.Width;
            var newHeight = (int)(resizedImage.Height * ratio);
            resizedImage.Resize((uint)maxWidth, (uint)newHeight);
        }

        // Set JPEG quality
        resizedImage.Quality = (uint)_jpegQuality;
        resizedImage.Format = MagickFormat.Jpeg;

        using var ms = new MemoryStream();
        await resizedImage.WriteAsync(ms);

        ms.Position = 0;
        var path = await _fileStorage.SaveFileAsync(ms, fileName, subfolder, scope);
        var size = ms.Length;

        return (path, size);
    }

    private async Task<(string path, long size)> SaveResizedImageAsync(
        SixLabors.ImageSharp.Image image, string subfolder, string fileName, int maxWidth, StorageScope scope)
    {
        using var resizedImage = image.Clone(ctx =>
        {
            if (image.Width > maxWidth)
            {
                var ratio = (double)maxWidth / image.Width;
                var newHeight = (int)(image.Height * ratio);
                ctx.Resize(maxWidth, newHeight);
            }
        });

        using var ms = new MemoryStream();
        var encoder = new JpegEncoder { Quality = _jpegQuality };
        await resizedImage.SaveAsync(ms, encoder);

        ms.Position = 0;
        var path = await _fileStorage.SaveFileAsync(ms, fileName, subfolder, scope);
        var size = ms.Length;

        return (path, size);
    }

    public async Task<bool> DeleteImageFilesAsync(MeterReadingImage image)
    {
        try
        {
            var optimizedDeleted = await _fileStorage.DeleteFileAsync(image.OptimizedPath);
            var thumbnailDeleted = await _fileStorage.DeleteFileAsync(image.ThumbnailPath);

            return optimizedDeleted && thumbnailDeleted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image files for image {ImageId}", image.Id);
            return false;
        }
    }

    public string GetImageMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".heif" => "image/heif",
            _ => "application/octet-stream"
        };
    }
}
