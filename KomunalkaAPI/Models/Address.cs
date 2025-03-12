using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class Address
{
    [Key]
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ZipCode { get; set; }
    public string? City { get; set; }
    public required string Street { get; set; }
    public string? Building { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    public required User User { get; set; }
    public List<ServiceCounter> ServiceCounters { get; set; }
    public List<AddressesServiceCategory> AddressesServiceCategories { get; set; }
}
