namespace KomunalkaAPI.Services.Image;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder, StorageScope scope = StorageScope.MeterReading);
    Task<bool> DeleteFileAsync(string filePath, StorageScope scope = StorageScope.MeterReading);
    Task<Stream?> GetFileAsync(string filePath, StorageScope scope = StorageScope.MeterReading);
    bool FileExists(string filePath, StorageScope scope = StorageScope.MeterReading);
    string GetFullPath(string relativePath, StorageScope scope = StorageScope.MeterReading);
}
