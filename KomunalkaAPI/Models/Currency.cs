using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

/// <summary>
/// Валюта для тарифів
/// </summary>
[Table("currencies")]
public class Currency
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(10)]
    [Column("code")]
    public required string Code { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name")]
    public required string Name { get; set; }

    [StringLength(10)]
    [Column("symbol")]
    public string? Symbol { get; set; }

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
}