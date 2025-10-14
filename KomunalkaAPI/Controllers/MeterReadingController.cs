using Asp.Versioning;
using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Background;
using KomunalkaAPI.Services.Image;
using KomunalkaAPI.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class MeterReadingController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _fileStorage;
    private readonly ImageProcessingService _imageProcessing;
    private readonly ILogger<MeterReadingController> _logger;
    private readonly IConfiguration _configuration;

    public MeterReadingController(
        IUnitOfWork unitOfWork,
        IFileStorageService fileStorage,
        ImageProcessingService imageProcessing,
        ILogger<MeterReadingController> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _fileStorage = fileStorage;
        _imageProcessing = imageProcessing;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Create a new meter reading with optional image
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<ServiceCounterValueDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMeterReading(
        [FromForm] CreateServiceCounterValueDto dto,
        [FromForm] [FileValidation] IFormFile? image = null)
    {
        try
        {
            // Create meter reading
            var reading = new ServiceCounterValue
            {
                ServiceCounterId = dto.ServiceCounterId,
                Value = dto.Value,
                ServiceCounter = (await _unitOfWork.ServiceCounters.GetByIdAsync(dto.ServiceCounterId))
                    ?? throw new InvalidOperationException("Service counter not found")
            };

            var entry = await _unitOfWork.ServiceCounterValues.AddAsync(reading);
            await _unitOfWork.CompleteAsync();

            var readingId = entry.Entity.Id;

            // If image provided, save temp file and queue for processing
            if (image != null)
            {
                var tempPath = await SaveTempFileAsync(image);

                // Create image record
                var imageRecord = new MeterReadingImage
                {
                    ServiceCounterValueId = readingId,
                    OptimizedPath = string.Empty,
                    ThumbnailPath = string.Empty,
                    MimeType = image.ContentType,
                    IsProcessed = false,
                    ServiceCounterValue = reading
                };

                var imageEntry = await _unitOfWork.MeterReadingImages.AddAsync(imageRecord);
                await _unitOfWork.CompleteAsync();

                // Queue for background processing
                await _imageProcessing.QueueImageProcessingAsync(imageEntry.Entity.Id, tempPath);

                _logger.LogInformation(
                    "Meter reading {ReadingId} created with image {ImageId} queued for processing",
                    readingId, imageEntry.Entity.Id);
            }

            var response = new ServiceCounterValueDto
            {
                Id = readingId,
                ServiceCounterId = reading.ServiceCounterId,
                Value = reading.Value,
                CreatedAt = reading.CreatedAt,
                UpdatedAt = reading.UpdatedAt
            };

            return CreatedAtAction(nameof(GetMeterReading), new { id = readingId },
                new ApiResponse<ServiceCounterValueDto> { Data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating meter reading");
            return BadRequest(new { error = "Failed to create meter reading", details = ex.Message });
        }
    }

    /// <summary>
    /// Get meter reading by ID with images
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ServiceCounterValueDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMeterReading(int id)
    {
        var reading = await _unitOfWork.ServiceCounterValues.GetByIdAsync(id);
        if (reading == null)
            return NotFound(new { error = "Meter reading not found" });

        var images = await _unitOfWork.MeterReadingImages.GetByServiceCounterValueIdAsync(id);

        var response = new ServiceCounterValueDto
        {
            Id = reading.Id,
            ServiceCounterId = reading.ServiceCounterId,
            Value = reading.Value,
            CreatedAt = reading.CreatedAt,
            UpdatedAt = reading.UpdatedAt,
            Images = images.Select(img => new MeterReadingImageDto
            {
                Id = img.Id,
                ServiceCounterValueId = img.ServiceCounterValueId,
                OptimizedUrl = $"/api/v1/meterreading/images/{img.Id}/optimized",
                ThumbnailUrl = $"/api/v1/meterreading/images/{img.Id}/thumbnail",
                OptimizedSizeInBytes = img.OptimizedSizeInBytes,
                ThumbnailSizeInBytes = img.ThumbnailSizeInBytes,
                Width = img.Width,
                Height = img.Height,
                MimeType = img.MimeType,
                IsProcessed = img.IsProcessed,
                CreatedAt = img.CreatedAt
            }).ToList()
        };

        return Ok(new ApiResponse<ServiceCounterValueDto> { Data = response });
    }

    /// <summary>
    /// Get optimized image
    /// </summary>
    [HttpGet("images/{id}/optimized")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOptimizedImage(int id)
    {
        var image = await _unitOfWork.MeterReadingImages.GetByIdAsync(id);
        if (image == null || !image.IsProcessed)
            return NotFound(new { error = "Image not found or not yet processed" });

        var fileStream = await _fileStorage.GetFileAsync(image.OptimizedPath);
        if (fileStream == null)
            return NotFound(new { error = "Image file not found" });

        return File(fileStream, image.MimeType);
    }

    /// <summary>
    /// Get thumbnail image
    /// </summary>
    [HttpGet("images/{id}/thumbnail")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetThumbnailImage(int id)
    {
        var image = await _unitOfWork.MeterReadingImages.GetByIdAsync(id);
        if (image == null || !image.IsProcessed)
            return NotFound(new { error = "Image not found or not yet processed" });

        var fileStream = await _fileStorage.GetFileAsync(image.ThumbnailPath);
        if (fileStream == null)
            return NotFound(new { error = "Image file not found" });

        return File(fileStream, image.MimeType);
    }

    /// <summary>
    /// Delete meter reading with images
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMeterReading(int id)
    {
        var reading = await _unitOfWork.ServiceCounterValues.GetByIdAsync(id);
        if (reading == null)
            return NotFound(new { error = "Meter reading not found" });

        // Delete associated images
        var images = await _unitOfWork.MeterReadingImages.GetByServiceCounterValueIdAsync(id);
        foreach (var image in images)
        {
            // Delete files from storage
            if (image.IsProcessed)
            {
                await _fileStorage.DeleteFileAsync(image.OptimizedPath);
                await _fileStorage.DeleteFileAsync(image.ThumbnailPath);
            }

            _unitOfWork.MeterReadingImages.Delete(image);
        }

        _unitOfWork.ServiceCounterValues.Delete(reading);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    private async Task<string> SaveTempFileAsync(IFormFile file)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
        await using var stream = new FileStream(tempPath, FileMode.Create);
        await file.CopyToAsync(stream);
        return tempPath;
    }
}
