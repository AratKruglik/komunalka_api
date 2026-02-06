using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

/// <summary>
/// Meter reading entry
/// </summary>
[Table("meter_readings")]
public class MeterReading
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("meter_id")]
    public int MeterId { get; set; }

    [Required]
    [Column("reading_value", TypeName = "decimal(18,2)")]
    public decimal ReadingValue { get; set; }

    [Required]
    [Column("reading_date")]
    public DateTime ReadingDate { get; set; }

    [Column("previous_reading_value", TypeName = "decimal(18,2)")]
    public decimal? PreviousReadingValue { get; set; }

    [Column("consumption", TypeName = "decimal(18,2)")]
    public decimal? Consumption { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("is_estimated")]
    public bool IsEstimated { get; set; } = false;

    [Column("tariff_id")]
    public int? TariffId { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(MeterId))]
    public virtual Meter Meter { get; set; } = null!;

    [ForeignKey(nameof(TariffId))]
    public virtual Tariff? Tariff { get; set; }

    public virtual ICollection<MeterReadingPhoto> Photos { get; set; } = new List<MeterReadingPhoto>();
}
