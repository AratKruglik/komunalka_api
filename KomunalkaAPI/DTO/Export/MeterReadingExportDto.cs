namespace KomunalkaAPI.DTO.Export;

public class MeterReadingExportDto
{
    public string Address { get; set; } = string.Empty;
    public string UtilityType { get; set; } = string.Empty;
    public string MeterName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public DateTime ReadingDate { get; set; }
    public decimal ReadingValue { get; set; }
    public decimal? PreviousValue { get; set; }
    public decimal? Consumption { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? TariffName { get; set; }
    public decimal? Cost { get; set; }
    public string Currency { get; set; } = string.Empty;
    public bool IsEstimated { get; set; }
}
