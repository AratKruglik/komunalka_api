using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class Region
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public required string Name { get; set; } // Назва області
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Навігаційні властивості
    public List<Address>? Addresses { get; set; }
}
