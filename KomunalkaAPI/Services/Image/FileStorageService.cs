namespace KomunalkaAPI.Services.Image;

public class FileStorageService : IFileStorageService
{
    private readonly string _meterReadingBasePath;
    private readonly string _avatarBasePath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IConfiguration configuration, ILogger<FileStorageService> logger)
    {
        _logger = logger;
        _meterReadingBasePath = configuration["ImageSettings:StoragePath"]
                                ?? throw new InvalidOperationException("ImageSettings:StoragePath is not configured");
        _avatarBasePath = configuration["AvatarImageSettings:StoragePath"]
                          ?? Path.Combine("wwwroot", "uploads", "avatars");
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subfolder, StorageScope scope = StorageScope.MeterReading)
    {
        try
        {
            var basePath = GetBasePath(scope);
            var directory = Path.Combine(basePath, subfolder);
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

    public Task<bool> DeleteFileAsync(string filePath, StorageScope scope = StorageScope.MeterReading)
    {
        try
        {
            var fullPath = GetFullPath(filePath, scope);
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

    public Task<Stream?> GetFileAsync(string filePath, StorageScope scope = StorageScope.MeterReading)
    {
        try
        {
            var fullPath = GetFullPath(filePath, scope);
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

    public bool FileExists(string filePath, StorageScope scope = StorageScope.MeterReading)
    {
        var fullPath = GetFullPath(filePath, scope);
        return File.Exists(fullPath);
    }

    public string GetFullPath(string relativePath, StorageScope scope = StorageScope.MeterReading)
    {
        var basePath = ResolveBasePathForRelativePath(relativePath, scope);
        return Path.GetFullPath(Path.Combine(basePath, relativePath));
    }

    private string GetBasePath(StorageScope scope)
    {
        return scope switch
        {
            StorageScope.Avatar => _avatarBasePath,
            _ => _meterReadingBasePath
        };
    }

    private string ResolveBasePathForRelativePath(string relativePath, StorageScope scope)
    {
        if (scope == StorageScope.Avatar)
        {
            return _avatarBasePath;
        }

        if (relativePath.StartsWith("avatars", StringComparison.OrdinalIgnoreCase))
        {
            return _avatarBasePath;
        }

        return _meterReadingBasePath;
    }
}
