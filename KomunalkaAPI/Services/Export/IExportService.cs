using KomunalkaAPI.DTO.Export;

namespace KomunalkaAPI.Services.Export;

public interface IExportService
{
    byte[] GenerateCsv(IEnumerable<MeterReadingExportDto> data);
    byte[] GeneratePdf(IEnumerable<MeterReadingExportDto> data);
}
