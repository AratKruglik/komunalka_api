namespace KomunalkaAPI.DTO;

public class TariffCalculationDto
{
    public int MeterId { get; set; }
    public string MeterName { get; set; } = string.Empty;
    public string UtilityType { get; set; } = string.Empty;
    public decimal Consumption { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal BaseRate { get; set; }
    public decimal? ServiceFee { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
    public decimal TotalCost { get; set; }
    public string TariffIdentifier { get; set; } = string.Empty;
    public DateTime TariffEffectiveFrom { get; set; }
    public DateTime? TariffEffectiveTo { get; set; }
    public decimal ConsumptionCost { get; set; }
    public decimal ServiceFeeCost { get; set; }
}
