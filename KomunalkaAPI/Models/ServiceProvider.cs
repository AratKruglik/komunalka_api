using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

/// <summary>
/// Service provider (utility company)
/// </summary>
[Table("service_providers")]
public class ServiceProvider
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name")]
    public required string Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [StringLength(50)]
    [Column("phone")]
    public string? Phone { get; set; }

    [StringLength(255)]
    [Column("email")]
    public string? Email { get; set; }

    [StringLength(500)]
    [Column("website")]
    public string? Website { get; set; }

    [Required]
    [Column("address_id")]
    public int AddressId { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    [ForeignKey(nameof(AddressId))]
    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Meter> Meters { get; set; } = new List<Meter>();
    public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}
