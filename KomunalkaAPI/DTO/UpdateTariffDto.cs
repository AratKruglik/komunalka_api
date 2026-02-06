using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class UpdateTariffDto
{
    public int? UtilityTypeId { get; set; }

    public int? CurrencyId { get; set; }

    [StringLength(50)]
    public string? PricingModel { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? BaseRate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ServiceFee { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public string? Notes { get; set; }
}
