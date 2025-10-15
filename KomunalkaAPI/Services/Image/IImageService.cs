using KomunalkaAPI.Models;

namespace KomunalkaAPI.Services.Image;

public interface IImageService
{
    Task<(string optimizedPath, string thumbnailPath, long optimizedSize, long thumbnailSize, int width, int height)>
        ProcessImageAsync(Stream imageStream, string fileName, int readingId);

    Task<bool> DeleteImageFilesAsync(MeterReadingImage image);
    string GetImageMimeType(string extension);
}
