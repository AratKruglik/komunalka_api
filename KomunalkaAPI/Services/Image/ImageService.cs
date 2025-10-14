using KomunalkaAPI.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace KomunalkaAPI.Services.Image;

public class ImageService : IImageService
{
    private readonly IFileStorageService _fileStorage;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ImageService> _logger;
    private readonly int _optimizedWidth;
    private readonly int _thumbnailWidth;
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
    }

    public async Task<(string optimizedPath, string thumbnailPath, long optimizedSize, long thumbnailSize, int width, int height)>
        ProcessImageAsync(Stream imageStream, string fileName, int readingId)
    {
        try
        {
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
            var subfolder = $"{DateTime.UtcNow.Year}/{DateTime.UtcNow.Month:D2}/{readingId}";

            // Process optimized image
            var optimizedFileName = $"{baseFileName}_{timestamp}_optimized.jpg";
            var (optimizedPath, optimizedSize) = await SaveResizedImageAsync(
                image, subfolder, optimizedFileName, _optimizedWidth);

            // Process thumbnail
            var thumbnailFileName = $"{baseFileName}_{timestamp}_thumbnail.jpg";
            var (thumbnailPath, thumbnailSize) = await SaveResizedImageAsync(
                image, subfolder, thumbnailFileName, _thumbnailWidth);

            return (optimizedPath, thumbnailPath, optimizedSize, thumbnailSize, originalWidth, originalHeight);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process image {FileName}", fileName);
            throw;
        }
    }

    private async Task<(string path, long size)> SaveResizedImageAsync(
        SixLabors.ImageSharp.Image image, string subfolder, string fileName, int maxWidth)
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
        var path = await _fileStorage.SaveFileAsync(ms, fileName, subfolder);
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
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
