using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

/// <summary>
/// Utility service meter
/// </summary>
[Table("meters")]
public class Meter
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("address_id")]
    public int AddressId { get; set; }

    [Required]
    [Column("utility_type_id")]
    public int UtilityTypeId { get; set; }

    [StringLength(255)]
    [Column("serial_number")]
    public string? SerialNumber { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [StringLength(255)]
    [Column("model_name")]
    public string? ModelName { get; set; }

    [StringLength(500)]
    [Column("location")]
    public string? Location { get; set; }

    [StringLength(500)]
    [Column("photo_path")]
    public string? PhotoPath { get; set; }

    [Column("installation_date")]
    public DateTime? InstallationDate { get; set; }

    [Column("initial_reading", TypeName = "decimal(18,2)")]
    public decimal? InitialReading { get; set; }

    [Column("service_provider_id")]
    public int? ServiceProviderId { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AddressId))]
    public virtual Address Address { get; set; } = null!;

    [ForeignKey(nameof(UtilityTypeId))]
    public virtual UtilityType UtilityType { get; set; } = null!;

    [ForeignKey(nameof(ServiceProviderId))]
    public virtual ServiceProvider? ServiceProvider { get; set; }

    public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
