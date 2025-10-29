using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

[Table("address_user")]
public class UserAddress
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }

    [ForeignKey("Address")]
    public int AddressId { get; set; }

    /// <summary>
    /// Чи є ця адреса основною для цього конкретного користувача
    /// </summary>
    public bool IsPrimary { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public required User User { get; set; }
    public required Address Address { get; set; }
}
