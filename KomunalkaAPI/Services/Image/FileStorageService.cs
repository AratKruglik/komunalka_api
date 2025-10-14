namespace KomunalkaAPI.Services.Image;

public class FileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _basePath = configuration["ImageSettings:StoragePath"]
                    ?? throw new InvalidOperationException("ImageSettings:StoragePath is not configured");
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder)
    {
        try
        {
            var directory = Path.Combine(_basePath, subfolder);
            Directory.CreateDirectory(directory);

            var filePath = Path.Combine(directory, fileName);
            var fullPath = Path.GetFullPath(filePath);

            await using var outputStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            await fileStream.CopyToAsync(outputStream);

            // Return relative path
            return Path.Combine(subfolder, fileName).Replace("\\", "/");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file {FileName}", fileName);
            throw;
        }
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                _logger.LogInformation("Deleted file {FilePath}", filePath);
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete file {FilePath}", filePath);
            throw;
        }
    }

    public Task<Stream?> GetFileAsync(string filePath)
    {
        try
        {
            var fullPath = GetFullPath(filePath);
            if (File.Exists(fullPath))
            {
                Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
                return Task.FromResult<Stream?>(stream);
            }
            return Task.FromResult<Stream?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get file {FilePath}", filePath);
            throw;
        }
    }

    public bool FileExists(string filePath)
    {
        var fullPath = GetFullPath(filePath);
        return File.Exists(fullPath);
    }

    public string GetFullPath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(_basePath, relativePath));
    }
}
