using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

public class Address
{
    [Key]
    public int Id { get; set; }
    
    [ForeignKey("User")]
    public int UserId { get; set; }
    
    [ForeignKey("Region")]
    public int RegionId { get; set; } // Область (зв'язок)
    
    [Required]
    [StringLength(100)]
    public required string City { get; set; } // Місто
    
    [Required]
    [StringLength(200)]
    public required string Street { get; set; } // Вулиця
    
    [Required]
    [StringLength(20)]
    public required string BuildingNumber { get; set; } // Номер будинку
    
    [StringLength(20)]
    public string? ApartmentNumber { get; set; } // Номер квартири/офісу
    
    [Required]
    [StringLength(5)]
    public string ZipCode { get; set; } // Поштовий індекс
    
    [StringLength(500)]
    public string? Notes { get; set; } // Примітки
    
    public bool IsPrimary { get; set; } // Чи є основною
    
    [ForeignKey("AddressType")]
    public int AddressTypeId { get; set; } // Тип адреси (зв'язок)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }
    
    // Навігаційні властивості
    public required User User { get; set; }
    public required Region Region { get; set; }
    public required AddressType AddressType { get; set; }
    public List<ServiceCounter>? ServiceCounters { get; set; }
    public List<AddressesServiceCategory>? AddressesServiceCategories { get; set; }
}
