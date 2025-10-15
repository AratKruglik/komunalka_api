using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class AddressesServiceCategory
{
    [Key]
    public int Id { get; set; }
    public int AddressId { get; set; }
    public int ServiceCategoryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public required Address Address { get; set; }
    public required ServiceCategory ServiceCategory { get; set; }
}