namespace KomunalkaAPI.Services.Image;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder);
    Task<bool> DeleteFileAsync(string filePath);
    Task<Stream?> GetFileAsync(string filePath);
    bool FileExists(string filePath);
    string GetFullPath(string relativePath);
}
