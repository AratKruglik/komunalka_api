using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Common;
using KomunalkaAPI.Services.Tariff;
using KomunalkaAPI.Services.Image;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Services.MeterReading;

public class MeterReadingService : IMeterReadingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITariffCalculationService _tariffCalculationService;
    private readonly IImageService _imageService;
    private readonly ILogger<MeterReadingService> _logger;

    public MeterReadingService(
        IUnitOfWork unitOfWork,
        ITariffCalculationService tariffCalculationService,
        IImageService imageService,
        ILogger<MeterReadingService> logger)
    {
        _unitOfWork = unitOfWork;
        _tariffCalculationService = tariffCalculationService;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<ServiceResult<BatchMeterReadingResponseDto>> CreateBatchAsync(
        int userId,
        BatchMeterReadingDto dto,
        Dictionary<int, IFormFile>? photosByMeterId)
    {
        // Verify user has access to address
        var userAddress = await _unitOfWork.UserAddresses.GetContext()
            .Set<UserAddress>()
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AddressId == dto.AddressId);

        if (userAddress == null)
        {
            return ServiceResult<BatchMeterReadingResponseDto>.ForbiddenResult(
                "You don't have access to this address");
        }

        // Load address for response
        var address = await _unitOfWork.Addresses.GetByIdAsync(dto.AddressId);
        if (address == null)
        {
            return ServiceResult<BatchMeterReadingResponseDto>.NotFoundResult("Address not found");
        }

        var createdReadings = new List<Models.MeterReading>();
        var calculationsData = new List<(Meter meter, decimal consumption, DateTime readingDate, int? tariffId)>();

        try
        {
            // Process each reading
            foreach (var readingDto in dto.Readings)
            {
                var readingDateUtc = DateTime.SpecifyKind(readingDto.ReadingDate, DateTimeKind.Utc);

                // Validate meter belongs to address and is active
                var meter = await _unitOfWork.Meters.GetContext()
                    .Set<Meter>()
                    .Include(m => m.UtilityType)
                    .FirstOrDefaultAsync(m => m.Id == readingDto.MeterId &&
                                            m.AddressId == dto.AddressId &&
                                            m.IsActive);

                if (meter == null)
                {
                    return ServiceResult<BatchMeterReadingResponseDto>.Fail(
                        $"Meter {readingDto.MeterId} not found or inactive for this address");
                }

                // Get previous reading
                var previousReading = await _unitOfWork.MeterReadings.GetLatestByMeterIdAsync(readingDto.MeterId);
                var previousValue = previousReading?.ReadingValue ?? meter.InitialReading ?? 0;

                // Validate current >= previous
                if (readingDto.ReadingValue < previousValue)
                {
                    return ServiceResult<BatchMeterReadingResponseDto>.Fail(
                        $"Current reading ({readingDto.ReadingValue}) for meter {meter.Name} cannot be less than previous reading ({previousValue})");
                }

                // Calculate consumption
                var consumption = readingDto.ReadingValue - previousValue;

                int? effectiveTariffId = readingDto.TariffId;
                if (effectiveTariffId == null)
                {
                    var resolvedTariff = await _tariffCalculationService.GetEffectiveTariffAsync(
                        meter.Id, readingDateUtc);
                    effectiveTariffId = resolvedTariff?.Id;
                }

                var meterReading = new Models.MeterReading
                {
                    MeterId = readingDto.MeterId,
                    ReadingValue = readingDto.ReadingValue,
                    ReadingDate = readingDateUtc,
                    PreviousReadingValue = previousValue,
                    Consumption = consumption,
                    Notes = readingDto.Notes,
                    IsEstimated = readingDto.IsEstimated,
                    TariffId = effectiveTariffId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _unitOfWork.MeterReadings.AddAsync(meterReading);
                createdReadings.Add(meterReading);
                calculationsData.Add((meter, consumption, readingDateUtc, effectiveTariffId));
            }

            // Save all readings in single transaction
            await _unitOfWork.CompleteAsync();

            // Process photos if provided
            if (photosByMeterId != null && photosByMeterId.Any())
            {
                foreach (var reading in createdReadings)
                {
                    if (photosByMeterId.TryGetValue(reading.MeterId, out var photo))
                    {
                        try
                        {
                            using var stream = photo.OpenReadStream();
                            var (optimizedPath, thumbnailPath, optimizedSize, thumbnailSize, width, height) =
                                await _imageService.ProcessImageAsync(stream, photo.FileName, reading.Id);

                            var meterReadingPhoto = new MeterReadingPhoto
                            {
                                MeterReadingId = reading.Id,
                                OptimizedPath = optimizedPath,
                                ThumbnailPath = thumbnailPath,
                                OptimizedSizeInBytes = optimizedSize,
                                ThumbnailSizeInBytes = thumbnailSize,
                                Width = width,
                                Height = height,
                                MimeType = _imageService.GetImageMimeType(Path.GetExtension(photo.FileName)),
                                IsProcessed = true,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow,
                                MeterReading = reading
                            };

                            await _unitOfWork.GetContext().Set<MeterReadingPhoto>().AddAsync(meterReadingPhoto);
                            reading.Photos.Add(meterReadingPhoto);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to process photo for meter {MeterId}", reading.MeterId);
                            // Continue processing other photos
                        }
                    }
                }

                // Save photos
                await _unitOfWork.CompleteAsync();
            }

            // Calculate costs
            var calculations = await _tariffCalculationService.CalculateBatchCostsAsync(calculationsData);

            // Prepare response
            var readingDtos = createdReadings.Select(r => new MeterReadingDto
            {
                Id = r.Id,
                MeterId = r.MeterId,
                ReadingValue = r.ReadingValue,
                ReadingDate = r.ReadingDate,
                PreviousReadingValue = r.PreviousReadingValue,
                Consumption = r.Consumption,
                Notes = r.Notes,
                IsEstimated = r.IsEstimated,
                TariffId = r.TariffId,
                TariffName = calculations.FirstOrDefault(c => c.MeterId == r.MeterId)?.TariffIdentifier,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                MeterName = calculationsData.FirstOrDefault(cd => cd.meter.Id == r.MeterId).meter?.Name,
                UtilityTypeName = calculationsData.FirstOrDefault(cd => cd.meter.Id == r.MeterId).meter?.UtilityType?.DisplayName,
                Photos = r.Photos?.Select(p => new MeterReadingPhotoDto
                {
                    Id = p.Id,
                    OptimizedUrl = $"/api/v1/meter-readings/photos/{p.Id}/optimized",
                    ThumbnailUrl = $"/api/v1/meter-readings/photos/{p.Id}/thumbnail",
                    Width = p.Width,
                    Height = p.Height,
                    IsProcessed = p.IsProcessed
                }).ToList() ?? new List<MeterReadingPhotoDto>()
            }).ToList();

            var totalCost = calculations.Sum(c => c.TotalCost);
            var primaryCurrency = calculations.FirstOrDefault()?.CurrencyCode ?? "UAH";
            var primarySymbol = calculations.FirstOrDefault()?.CurrencySymbol ?? "₴";

            var response = new BatchMeterReadingResponseDto
            {
                AddressId = dto.AddressId,
                AddressDisplay = $"{address.Street}, {address.BuildingNumber}" +
                                (address.ApartmentNumber != null ? $", кв. {address.ApartmentNumber}" : ""),
                Readings = readingDtos,
                Calculations = calculations,
                TotalCost = totalCost,
                CurrencyCode = primaryCurrency,
                CurrencySymbol = primarySymbol,
                SubmittedAt = DateTime.UtcNow
            };

            return ServiceResult<BatchMeterReadingResponseDto>.Ok(response);
        }
        catch (Exception ex)
        {
            return ServiceResult<BatchMeterReadingResponseDto>.Fail($"Error creating readings: {ex.Message}");
        }
    }

    public async Task<ServiceResult<IEnumerable<MeterReadingDto>>> GetByAddressIdAsync(
        int userId,
        int addressId,
        DateTime? from = null,
        DateTime? to = null)
    {
        // Verify user has access to address
        var userAddress = await _unitOfWork.UserAddresses.GetContext()
            .Set<UserAddress>()
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AddressId == addressId);

        if (userAddress == null)
        {
            return ServiceResult<IEnumerable<MeterReadingDto>>.ForbiddenResult(
                "You don't have access to this address");
        }

        IEnumerable<Models.MeterReading> readings;

        if (from.HasValue && to.HasValue)
        {
            readings = await _unitOfWork.MeterReadings.GetByAddressAndDateRangeAsync(
                addressId,
                DateTime.SpecifyKind(from.Value, DateTimeKind.Utc),
                DateTime.SpecifyKind(to.Value, DateTimeKind.Utc));
        }
        else
        {
            readings = await _unitOfWork.MeterReadings.GetByAddressIdAsync(addressId);
        }

        var dtos = readings.Select(r => new MeterReadingDto
        {
            Id = r.Id,
            MeterId = r.MeterId,
            ReadingValue = r.ReadingValue,
            ReadingDate = r.ReadingDate,
            PreviousReadingValue = r.PreviousReadingValue,
            Consumption = r.Consumption,
            Notes = r.Notes,
            IsEstimated = r.IsEstimated,
            TariffId = r.TariffId,
            TariffName = r.Tariff?.Name,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            MeterName = r.Meter?.Name,
            UtilityTypeName = r.Meter?.UtilityType?.DisplayName,
            Photos = r.Photos?.Select(p => new MeterReadingPhotoDto
            {
                Id = p.Id,
                OptimizedUrl = $"/api/v1/meter-readings/photos/{p.Id}/optimized",
                ThumbnailUrl = $"/api/v1/meter-readings/photos/{p.Id}/thumbnail",
                Width = p.Width,
                Height = p.Height,
                IsProcessed = p.IsProcessed
            }).ToList()
        }).ToList();

        return ServiceResult<IEnumerable<MeterReadingDto>>.Ok(dtos);
    }

    public async Task<ServiceResult<MeterReadingDto>> GetByIdAsync(int userId, int readingId)
    {
        var reading = await _unitOfWork.MeterReadings.GetContext()
            .Set<Models.MeterReading>()
            .Include(r => r.Meter)
                .ThenInclude(m => m.UtilityType)
            .Include(r => r.Meter)
                .ThenInclude(m => m.Address)
            .Include(r => r.Photos)
            .Include(r => r.Tariff)
            .FirstOrDefaultAsync(r => r.Id == readingId);

        if (reading == null)
        {
            return ServiceResult<MeterReadingDto>.NotFoundResult("Reading not found");
        }

        // Verify user has access
        var userAddress = await _unitOfWork.UserAddresses.GetContext()
            .Set<UserAddress>()
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AddressId == reading.Meter.AddressId);

        if (userAddress == null)
        {
            return ServiceResult<MeterReadingDto>.ForbiddenResult("You don't have access to this reading");
        }

        var dto = new MeterReadingDto
        {
            Id = reading.Id,
            MeterId = reading.MeterId,
            ReadingValue = reading.ReadingValue,
            ReadingDate = reading.ReadingDate,
            PreviousReadingValue = reading.PreviousReadingValue,
            Consumption = reading.Consumption,
            Notes = reading.Notes,
            IsEstimated = reading.IsEstimated,
            TariffId = reading.TariffId,
            TariffName = reading.Tariff?.Name,
            CreatedAt = reading.CreatedAt,
            UpdatedAt = reading.UpdatedAt,
            MeterName = reading.Meter?.Name,
            UtilityTypeName = reading.Meter?.UtilityType?.DisplayName,
            Photos = reading.Photos?.Select(p => new MeterReadingPhotoDto
            {
                Id = p.Id,
                OptimizedUrl = $"/api/v1/meter-readings/photos/{p.Id}/optimized",
                ThumbnailUrl = $"/api/v1/meter-readings/photos/{p.Id}/thumbnail",
                Width = p.Width,
                Height = p.Height,
                IsProcessed = p.IsProcessed
            }).ToList()
        };

        return ServiceResult<MeterReadingDto>.Ok(dto);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int userId, int readingId)
    {
        var reading = await _unitOfWork.MeterReadings.GetContext()
            .Set<Models.MeterReading>()
            .Include(r => r.Meter)
            .Include(r => r.Photos)
            .FirstOrDefaultAsync(r => r.Id == readingId);

        if (reading == null)
        {
            return ServiceResult<bool>.NotFoundResult("Reading not found");
        }

        // Verify user has access
        var userAddress = await _unitOfWork.UserAddresses.GetContext()
            .Set<UserAddress>()
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AddressId == reading.Meter.AddressId);

        if (userAddress == null)
        {
            return ServiceResult<bool>.ForbiddenResult("You don't have access to this reading");
        }

        _unitOfWork.MeterReadings.Delete(reading);
        await _unitOfWork.CompleteAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<IEnumerable<DTO.Export.MeterReadingExportDto>>> GetReadingsForExportAsync(
        int userId,
        DTO.Export.ExportRequestDto request)
    {
        // 1. Get filtered addresses for user
        var query = _unitOfWork.UserAddresses.GetContext()
            .Set<UserAddress>()
            .Include(ua => ua.Address)
            .Where(ua => ua.UserId == userId);

        if (request.AddressIds != null && request.AddressIds.Any())
        {
            query = query.Where(ua => request.AddressIds.Contains(ua.AddressId));
        }

        var addresses = await query.Select(ua => ua.Address).ToListAsync();

        if (!addresses.Any())
        {
            return ServiceResult<IEnumerable<DTO.Export.MeterReadingExportDto>>.Ok(new List<DTO.Export.MeterReadingExportDto>());
        }

        var addressIds = addresses.Select(a => a.Id).ToList();

        // 2. Fetch readings for these addresses in range
        var readings = await _unitOfWork.MeterReadings.GetContext()
            .Set<Models.MeterReading>()
            .Include(mr => mr.Meter)
                .ThenInclude(m => m.UtilityType)
            .Include(mr => mr.Meter)
                .ThenInclude(m => m.Address)
            .Include(mr => mr.Tariff)
                .ThenInclude(t => t!.Currency)
            .Where(mr => addressIds.Contains(mr.Meter.AddressId) &&
                         mr.ReadingDate >= DateTime.SpecifyKind(request.FromDate, DateTimeKind.Utc) &&
                         mr.ReadingDate <= DateTime.SpecifyKind(request.ToDate, DateTimeKind.Utc))
            .OrderBy(mr => mr.Meter.AddressId)
            .ThenBy(mr => mr.MeterId)
            .ThenBy(mr => mr.ReadingDate)
            .ToListAsync();
            
        // 3. Map to DTO
        var dtos = readings.Select(r => new DTO.Export.MeterReadingExportDto
        {
            Address = $"{r.Meter!.Address.City}, {r.Meter.Address.Street} {r.Meter.Address.BuildingNumber}" + 
                      (r.Meter.Address.ApartmentNumber != null ? $", {r.Meter.Address.ApartmentNumber}" : ""),
            UtilityType = r.Meter.UtilityType.DisplayName,
            MeterName = r.Meter.Name,
            SerialNumber = r.Meter.SerialNumber ?? "",
            ReadingDate = r.ReadingDate,
            ReadingValue = r.ReadingValue,
            PreviousValue = r.PreviousReadingValue,
            Consumption = r.Consumption,
            Unit = r.Meter.UtilityType.Unit,
            TariffName = r.Tariff?.Name ?? "",
            Cost = null, // Can be calculated if needed, but not stored directly usually
            Currency = r.Tariff?.Currency?.Code ?? "",
            IsEstimated = r.IsEstimated
        }).ToList();

        return ServiceResult<IEnumerable<DTO.Export.MeterReadingExportDto>>.Ok(dtos);
    }
}
