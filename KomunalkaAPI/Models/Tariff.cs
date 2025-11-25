using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

/// <summary>
/// Tariff for meter
/// </summary>
[Table("tariffs")]
public class Tariff
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("service_provider_id")]
    public int ServiceProviderId { get; set; }

    [Required]
    [Column("utility_type_id")]
    public int UtilityTypeId { get; set; }

    [Required]
    [Column("currency_id")]
    public int CurrencyId { get; set; }

    [Required]
    [StringLength(50)]
    [Column("pricing_model")]
    public required string PricingModel { get; set; }

    [Required]
    [Column("base_rate", TypeName = "decimal(18,2)")]
    public decimal BaseRate { get; set; }

    [Column("service_fee", TypeName = "decimal(18,2)")]
    public decimal? ServiceFee { get; set; }

    [Required]
    [Column("effective_from")]
    public DateTime EffectiveFrom { get; set; }

    [Column("effective_to")]
    public DateTime? EffectiveTo { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(ServiceProviderId))]
    public virtual ServiceProvider ServiceProvider { get; set; } = null!;

    [ForeignKey(nameof(UtilityTypeId))]
    public virtual UtilityType UtilityType { get; set; } = null!;

    [ForeignKey(nameof(CurrencyId))]
    public virtual Currency Currency { get; set; } = null!;
}