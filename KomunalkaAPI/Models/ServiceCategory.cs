using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class ServiceCategory
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public required List<Address> Addresses { get; set; } = new();
}