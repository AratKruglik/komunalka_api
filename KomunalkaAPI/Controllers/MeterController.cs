using Asp.Versioning;
using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class MeterController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MeterController> _logger;

    public MeterController(
        IUnitOfWork unitOfWork,
        ILogger<MeterController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all meters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MeterDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var meters = await _unitOfWork.Meters.GetAllAsync();
        var meterDtos = meters.Select(m => MapToDto(m));

        return Ok(new ApiResponse<IEnumerable<MeterDto>> { Data = meterDtos });
    }

    /// <summary>
    /// Get meter by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MeterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
    {
        var meter = await _unitOfWork.Meters.GetByIdAsync(id);
        if (meter == null)
            return NotFound(new { error = "Meter not found" });

        return Ok(new ApiResponse<MeterDto> { Data = MapToDto(meter) });
    }

    /// <summary>
    /// Get meters by address ID
    /// </summary>
    [HttpGet("address/{addressId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MeterDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAddressId(int addressId)
    {
        var meters = await _unitOfWork.Meters.GetByAddressIdAsync(addressId);
        var meterDtos = meters.Select(m => MapToDto(m));

        return Ok(new ApiResponse<IEnumerable<MeterDto>> { Data = meterDtos });
    }

    /// <summary>
    /// Get active meters
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<MeterDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActive()
    {
        var meters = await _unitOfWork.Meters.GetActiveMetersAsync();
        var meterDtos = meters.Select(m => MapToDto(m));

        return Ok(new ApiResponse<IEnumerable<MeterDto>> { Data = meterDtos });
    }

    /// <summary>
    /// Create a new meter
    /// </summary>
    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ApiResponse<MeterDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMeterJsonDto dto)
    {
        try
        {
            // Validate that service provider exists and is active
            var serviceProvider = await _unitOfWork.ServiceProviders.GetByIdAsync(dto.ServiceProviderId);
            if (serviceProvider == null)
                return BadRequest(new { error = "Service provider not found" });

            if (!serviceProvider.IsActive)
                return BadRequest(new { error = "Service provider is not active" });

            // Optionally validate that utility types match
            if (serviceProvider.UtilityTypeId != dto.UtilityTypeId)
                return BadRequest(new { error = "Service provider utility type does not match meter utility type" });

            var meter = new Models.Meter
            {
                AddressId = dto.AddressId,
                UtilityTypeId = dto.UtilityTypeId,
                SerialNumber = dto.SerialNumber,
                Name = dto.Name,
                Description = dto.Description,
                ModelName = dto.ModelName,
                Location = dto.Location,
                PhotoPath = null,
                InstallationDate = dto.InstallationDate,
                InitialReading = dto.InitialReading,
                ServiceProviderId = dto.ServiceProviderId,
                Notes = dto.Notes,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var entry = await _unitOfWork.Meters.AddAsync(meter);
            await _unitOfWork.CompleteAsync();

            var createdMeter = await _unitOfWork.Meters.GetByIdAsync(entry.Entity.Id);

            return CreatedAtAction(nameof(GetById), new { id = entry.Entity.Id },
                new ApiResponse<MeterDto> { Data = MapToDto(createdMeter!) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating meter");
            return BadRequest(new { error = "Failed to create meter", details = ex.Message });
        }
    }

    /// <summary>
    /// Upload photo for a meter
    /// </summary>
    [HttpPost("{id}/photo")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(ApiResponse<MeterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadPhoto(int id, [FromForm] IFormFile photo)
    {
        try
        {
            var meter = await _unitOfWork.Meters.GetByIdAsync(id);
            if (meter == null)
                return NotFound(new { error = "Meter not found" });

            if (photo == null || photo.Length == 0)
                return BadRequest(new { error = "Photo file is required" });

            // Delete old photo if exists
            if (!string.IsNullOrEmpty(meter.PhotoPath))
            {
                DeletePhoto(meter.PhotoPath);
            }

            // Save new photo
            var photoPath = await SavePhotoAsync(photo);
            meter.PhotoPath = photoPath;
            meter.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Meters.Update(meter);
            await _unitOfWork.CompleteAsync();

            var updatedMeter = await _unitOfWork.Meters.GetByIdAsync(id);

            return Ok(new ApiResponse<MeterDto> { Data = MapToDto(updatedMeter!) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading meter photo");
            return BadRequest(new { error = "Failed to upload photo", details = ex.Message });
        }
    }

    /// <summary>
    /// Update a meter
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<MeterDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMeterDto dto)
    {
        try
        {
            var meter = await _unitOfWork.Meters.GetByIdAsync(id);
            if (meter == null)
                return NotFound(new { error = "Meter not found" });

            // Update fields if provided
            if (dto.SerialNumber != null)
                meter.SerialNumber = dto.SerialNumber;

            if (dto.Name != null)
                meter.Name = dto.Name;

            if (dto.Description != null)
                meter.Description = dto.Description;

            if (dto.ModelName != null)
                meter.ModelName = dto.ModelName;

            if (dto.Location != null)
                meter.Location = dto.Location;

            if (dto.InstallationDate.HasValue)
                meter.InstallationDate = dto.InstallationDate;

            if (dto.InitialReading.HasValue)
                meter.InitialReading = dto.InitialReading;

            if (dto.ServiceProviderId.HasValue)
            {
                var provider = await _unitOfWork.ServiceProviders.GetByIdAsync(dto.ServiceProviderId.Value);
                if (provider == null)
                    return BadRequest(new { error = "Service provider not found" });
                meter.ServiceProviderId = dto.ServiceProviderId;
            }

            if (dto.Notes != null)
                meter.Notes = dto.Notes;

            if (dto.IsActive.HasValue)
                meter.IsActive = dto.IsActive.Value;

            meter.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Meters.Update(meter);
            await _unitOfWork.CompleteAsync();

            var updatedMeter = await _unitOfWork.Meters.GetByIdAsync(id);

            return Ok(new ApiResponse<MeterDto> { Data = MapToDto(updatedMeter!) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating meter");
            return BadRequest(new { error = "Failed to update meter", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete a meter
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var meter = await _unitOfWork.Meters.GetByIdAsync(id);
        if (meter == null)
            return NotFound(new { error = "Meter not found" });

        // Delete photo if exists
        if (!string.IsNullOrEmpty(meter.PhotoPath))
        {
            DeletePhoto(meter.PhotoPath);
        }

        _unitOfWork.Meters.Delete(meter);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    private MeterDto MapToDto(Models.Meter meter)
    {
        return new MeterDto
        {
            Id = meter.Id,
            AddressId = meter.AddressId,
            UtilityTypeId = meter.UtilityTypeId,
            SerialNumber = meter.SerialNumber,
            Name = meter.Name,
            Description = meter.Description,
            ModelName = meter.ModelName,
            Location = meter.Location,
            PhotoPath = meter.PhotoPath,
            InstallationDate = meter.InstallationDate,
            InitialReading = meter.InitialReading,
            ServiceProviderId = meter.ServiceProviderId ?? 0,
            Notes = meter.Notes,
            IsActive = meter.IsActive,
            CreatedAt = meter.CreatedAt,
            UpdatedAt = meter.UpdatedAt,
            UtilityTypeName = meter.UtilityType?.DisplayName,
            ServiceProviderName = meter.ServiceProvider?.Name
        };
    }

    private async Task<string> SavePhotoAsync(IFormFile photo)
    {
        // Create uploads directory if it doesn't exist
        var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "meters");
        Directory.CreateDirectory(uploadsPath);

        // Generate unique filename
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(photo.FileName)}";
        var filePath = Path.Combine(uploadsPath, fileName);

        // Save file
        await using var stream = new FileStream(filePath, FileMode.Create);
        await photo.CopyToAsync(stream);

        return Path.Combine("uploads", "meters", fileName);
    }

    private void DeletePhoto(string photoPath)
    {
        try
        {
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), photoPath);
            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete photo: {PhotoPath}", photoPath);
        }
    }
}
