using System.Security.Claims;
using System.Text.Json;
using Asp.Versioning;
using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.MeterReading;
using KomunalkaAPI.Services.Image;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/meter-readings")]
[Authorize]
public class BatchMeterReadingsController : ControllerBase
{
    private readonly IMeterReadingService _meterReadingService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BatchMeterReadingsController> _logger;

    public BatchMeterReadingsController(
        IMeterReadingService meterReadingService,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork,
        ILogger<BatchMeterReadingsController> logger)
    {
        _meterReadingService = meterReadingService;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Submit batch of meter readings for an address
    /// </summary>
    [HttpPost("batch")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<BatchMeterReadingResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateBatch([FromForm] BatchMeterReadingRequestForm form)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user credentials" });
        }

        // Deserialize readings from JSON
        BatchMeterReadingDto? batchDto;
        try
        {
            batchDto = JsonSerializer.Deserialize<BatchMeterReadingDto>(form.ReadingsJson, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (batchDto == null)
            {
                return BadRequest(new { error = "Invalid readings data" });
            }
        }
        catch (JsonException ex)
        {
            return BadRequest(new { error = $"Invalid JSON format: {ex.Message}" });
        }

        // Parse photos by meter ID
        Dictionary<int, IFormFile>? photosByMeterId = null;
        if (form.Photos != null && form.Photos.Any())
        {
            photosByMeterId = new Dictionary<int, IFormFile>();
            foreach (var photo in form.Photos)
            {
                // Expected format: "photo_{meterId}"
                var fileName = photo.FileName;
                if (fileName.StartsWith("photo_") && int.TryParse(fileName.Substring(6).Split('.')[0], out var meterId))
                {
                    photosByMeterId[meterId] = photo;
                }
            }
        }

        var result = await _meterReadingService.CreateBatchAsync(userId, batchDto, photosByMeterId);

        if (result.NotFound)
        {
            return NotFound(new { errors = result.Errors });
        }

        if (result.Forbidden)
        {
            return Forbid();
        }

        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return CreatedAtAction(
            nameof(GetByAddress),
            new { addressId = batchDto.AddressId },
            new ApiResponse<BatchMeterReadingResponseDto> { Data = result.Data });
    }

    /// <summary>
    /// Get all meter readings for an address
    /// </summary>
    [HttpGet("address/{addressId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MeterReadingDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAddress(
        int addressId,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user credentials" });
        }

        var result = await _meterReadingService.GetByAddressIdAsync(userId, addressId, from, to);

        if (result.NotFound)
        {
            return NotFound(new { errors = result.Errors });
        }

        if (result.Forbidden)
        {
            return Forbid();
        }

        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new ApiResponse<IEnumerable<MeterReadingDto>> { Data = result.Data });
    }

    /// <summary>
    /// Get single meter reading by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MeterReadingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user credentials" });
        }

        var result = await _meterReadingService.GetByIdAsync(userId, id);

        if (result.NotFound)
        {
            return NotFound(new { errors = result.Errors });
        }

        if (result.Forbidden)
        {
            return Forbid();
        }

        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return Ok(new ApiResponse<MeterReadingDto> { Data = result.Data });
    }

    /// <summary>
    /// Delete meter reading
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Invalid user credentials" });
        }

        var result = await _meterReadingService.DeleteAsync(userId, id);

        if (result.NotFound)
        {
            return NotFound(new { errors = result.Errors });
        }

        if (result.Forbidden)
        {
            return Forbid();
        }

        if (!result.Success)
        {
            return BadRequest(new { errors = result.Errors });
        }

        return NoContent();
    }

    /// <summary>
    /// Get optimized photo for meter reading
    /// </summary>
    [HttpGet("photos/{id}/optimized")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOptimizedPhoto(int id)
    {
        var photo = await _unitOfWork.GetContext()
            .Set<Models.MeterReadingPhoto>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (photo == null || !photo.IsProcessed)
        {
            return NotFound(new { error = "Photo not found or not yet processed" });
        }

        var fileStream = await _fileStorageService.GetFileAsync(photo.OptimizedPath);
        if (fileStream == null)
        {
            return NotFound(new { error = "Photo file not found" });
        }

        return File(fileStream, photo.MimeType);
    }

    /// <summary>
    /// Get thumbnail photo for meter reading
    /// </summary>
    [HttpGet("photos/{id}/thumbnail")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetThumbnailPhoto(int id)
    {
        var photo = await _unitOfWork.GetContext()
            .Set<Models.MeterReadingPhoto>()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (photo == null || !photo.IsProcessed)
        {
            return NotFound(new { error = "Photo not found or not yet processed" });
        }

        var fileStream = await _fileStorageService.GetFileAsync(photo.ThumbnailPath);
        if (fileStream == null)
        {
            return NotFound(new { error = "Photo file not found" });
        }

        return File(fileStream, photo.MimeType);
    }
}

/// <summary>
/// Form model for batch meter reading submission
/// </summary>
public class BatchMeterReadingRequestForm
{
    /// <summary>
    /// Address ID
    /// </summary>
    public int AddressId { get; set; }

    /// <summary>
    /// JSON string containing array of meter readings
    /// </summary>
    public required string ReadingsJson { get; set; }

    /// <summary>
    /// Optional photos for meters (file names should be photo_{meterId}.jpg)
    /// </summary>
    public IFormFileCollection? Photos { get; set; }
}
