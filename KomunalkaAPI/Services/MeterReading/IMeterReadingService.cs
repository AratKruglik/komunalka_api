using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.MeterReading;

public interface IMeterReadingService
{
    Task<ServiceResult<BatchMeterReadingResponseDto>> CreateBatchAsync(
        int userId,
        BatchMeterReadingDto dto,
        Dictionary<int, IFormFile>? photosByMeterId);

    Task<ServiceResult<IEnumerable<MeterReadingDto>>> GetByAddressIdAsync(
        int userId,
        int addressId,
        DateTime? from = null,
        DateTime? to = null);

    Task<ServiceResult<MeterReadingDto>> GetByIdAsync(int userId, int readingId);

    Task<ServiceResult<bool>> DeleteAsync(int userId, int readingId);

    Task<ServiceResult<IEnumerable<DTO.Export.MeterReadingExportDto>>> GetReadingsForExportAsync(
        int userId,
        DTO.Export.ExportRequestDto request);
}
