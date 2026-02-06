using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class CreateTariffDto
{
    public int? UtilityTypeId { get; set; }

    public int? CurrencyId { get; set; }

    [Required]
    [StringLength(100)]
    public required string Name { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal BaseRate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ServiceFee { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public string? Notes { get; set; }
}
