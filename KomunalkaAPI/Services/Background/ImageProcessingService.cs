using System.Threading.Channels;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Image;

namespace KomunalkaAPI.Services.Background;

public class ImageProcessingService : BackgroundService
{
    private readonly Channel<ImageProcessingJob> _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ImageProcessingService> _logger;

    public ImageProcessingService(
        IServiceProvider serviceProvider,
        ILogger<ImageProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _queue = Channel.CreateUnbounded<ImageProcessingJob>();
    }

    public async Task QueueImageProcessingAsync(int imageId, string tempFilePath)
    {
        var job = new ImageProcessingJob
        {
            ImageId = imageId,
            TempFilePath = tempFilePath,
            QueuedAt = DateTime.UtcNow
        };

        await _queue.Writer.WriteAsync(job);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Image Processing Service started");

        await foreach (var job in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessImageJobAsync(job, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing image job for ImageId {ImageId}", job.ImageId);
            }
        }
    }

    private async Task ProcessImageJobAsync(ImageProcessingJob job, CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var imageService = scope.ServiceProvider.GetRequiredService<IImageService>();
        var fileStorage = scope.ServiceProvider.GetRequiredService<IFileStorageService>();

        try
        {
            var image = await unitOfWork.MeterReadingImages.GetByIdAsync(job.ImageId);
            if (image == null)
            {
                _logger.LogWarning("Image {ImageId} not found in database", job.ImageId);
                // Clean up temp file
                if (File.Exists(job.TempFilePath))
                {
                    File.Delete(job.TempFilePath);
                }
                return;
            }

            // Process the image from temp file
            await using var fileStream = File.OpenRead(job.TempFilePath);
            var fileName = Path.GetFileName(job.TempFilePath);

            var (optimizedPath, thumbnailPath, optimizedSize, thumbnailSize, width, height) =
                await imageService.ProcessImageAsync(fileStream, fileName, image.ServiceCounterValueId);

            // Update image record
            image.OptimizedPath = optimizedPath;
            image.ThumbnailPath = thumbnailPath;
            image.OptimizedSizeInBytes = optimizedSize;
            image.ThumbnailSizeInBytes = thumbnailSize;
            image.Width = width;
            image.Height = height;
            image.IsProcessed = true;
            image.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.CompleteAsync();

            // Clean up temp file
            if (File.Exists(job.TempFilePath))
            {
                File.Delete(job.TempFilePath);
            }

            _logger.LogInformation(
                "Successfully processed image {ImageId}, optimized: {OptimizedSize} bytes, thumbnail: {ThumbnailSize} bytes",
                job.ImageId, optimizedSize, thumbnailSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process image {ImageId}", job.ImageId);

            // Mark image as failed (we'll keep IsProcessed = false)
            try
            {
                var image = await unitOfWork.MeterReadingImages.GetByIdAsync(job.ImageId);
                if (image != null)
                {
                    // You might want to add a FailureReason field to track this
                    image.UpdatedAt = DateTime.UtcNow;
                    await unitOfWork.CompleteAsync();
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to update image status for {ImageId}", job.ImageId);
            }

            // Clean up temp file
            if (File.Exists(job.TempFilePath))
            {
                File.Delete(job.TempFilePath);
            }

            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Image Processing Service stopping...");
        _queue.Writer.Complete();
        await base.StopAsync(cancellationToken);
    }
}

public class ImageProcessingJob
{
    public int ImageId { get; set; }
    public required string TempFilePath { get; set; }
    public DateTime QueuedAt { get; set; }
}
