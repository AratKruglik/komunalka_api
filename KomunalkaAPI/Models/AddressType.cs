using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class AddressType
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public required string Name { get; set; } // Назва типу (Квартира, Приватний будинок, Офіс)
    
    [StringLength(200)]
    public string? Description { get; set; } // Опис типу
    
    [StringLength(100)]
    public string? Icon { get; set; } // Іконка (може бути назва іконки, Unicode, або клас CSS)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Навігаційні властивості
    public List<Address>? Addresses { get; set; }
}
