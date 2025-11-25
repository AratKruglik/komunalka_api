using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

/// <summary>
/// Utility service type (electricity, gas, water, etc.)
/// </summary>
[Table("utility_types")]
public class UtilityType
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("slug")]
    public required string Slug { get; set; }

    [Required]
    [StringLength(255)]
    [Column("display_name")]
    public required string DisplayName { get; set; }

    [Required]
    [StringLength(50)]
    [Column("unit")]
    public required string Unit { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Meter> Meters { get; set; } = new List<Meter>();
    public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
