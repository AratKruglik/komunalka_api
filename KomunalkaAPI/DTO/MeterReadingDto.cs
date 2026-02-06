namespace KomunalkaAPI.DTO;

public class MeterReadingDto
{
    public int Id { get; set; }
    public int MeterId { get; set; }
    public decimal ReadingValue { get; set; }
    public DateTime ReadingDate { get; set; }
    public decimal? PreviousReadingValue { get; set; }
    public decimal? Consumption { get; set; }
    public string? Notes { get; set; }
    public bool IsEstimated { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public string? MeterName { get; set; }
    public string? UtilityTypeName { get; set; }
    public string? Unit { get; set; }

    public int? TariffId { get; set; }
    public string? TariffName { get; set; }

    public List<MeterReadingPhotoDto>? Photos { get; set; }
}
