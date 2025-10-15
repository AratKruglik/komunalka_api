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

    public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
