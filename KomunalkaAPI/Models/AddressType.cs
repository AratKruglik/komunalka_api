using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class AddressType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Name { get; set; } // Type name (Apartment, Private house, Office)

    [StringLength(200)]
    public string? Description { get; set; } // Type description

    [StringLength(100)]
    public string? Icon { get; set; } // Icon (can be icon name, Unicode, or CSS class)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public List<Address>? Addresses { get; set; }
}
