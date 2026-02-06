using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class CreateMeterReadingDto
{
    [Required(ErrorMessage = "Meter ID is required")]
    public int MeterId { get; set; }

    [Required(ErrorMessage = "Reading value is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Reading value must be non-negative")]
    public decimal ReadingValue { get; set; }

    [Required(ErrorMessage = "Reading date is required")]
    public DateTime ReadingDate { get; set; }

    [MaxLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; }

    public bool IsEstimated { get; set; } = false;

    public int? TariffId { get; set; }
}
