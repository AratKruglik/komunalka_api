namespace KomunalkaAPI.DTO;

public class BatchMeterReadingResponseDto
{
    public int AddressId { get; set; }
    public string AddressDisplay { get; set; } = string.Empty;
    public List<MeterReadingDto> Readings { get; set; } = new();
    public List<TariffCalculationDto> Calculations { get; set; } = new();
    public decimal TotalCost { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
}
