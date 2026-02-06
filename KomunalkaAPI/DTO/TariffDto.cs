namespace KomunalkaAPI.DTO;

public class TariffDto
{
    public int Id { get; set; }
    public int ServiceProviderId { get; set; }
    public int UtilityTypeId { get; set; }
    public int CurrencyId { get; set; }
    public required string Name { get; set; }
    public decimal BaseRate { get; set; }
    public decimal? ServiceFee { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? Notes { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Related data
    public string? UtilityTypeName { get; set; }
    public string? CurrencyCode { get; set; }
    public string? CurrencySymbol { get; set; }
}
