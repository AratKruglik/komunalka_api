using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

public class Address
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Region")]
    public int RegionId { get; set; } // Region (relationship)

    [Required]
    [StringLength(100)]
    public required string City { get; set; } // City

    [Required]
    [StringLength(200)]
    public required string Street { get; set; } // Street

    [Required]
    [StringLength(20)]
    public required string BuildingNumber { get; set; } // Building number

    [StringLength(20)]
    public string? ApartmentNumber { get; set; } // Apartment/office number

    [Required]
    [StringLength(5)]
    public required string ZipCode { get; set; } // Postal code

    [StringLength(500)]
    public string? Notes { get; set; } // Notes

    [ForeignKey("AddressType")]
    public int AddressTypeId { get; set; } // Address type (relationship)
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public List<UserAddress>? UserAddresses { get; set; }
    public required Region Region { get; set; }
    public required AddressType AddressType { get; set; }
    public List<ServiceCounter>? ServiceCounters { get; set; }
    public List<AddressesServiceCategory>? AddressesServiceCategories { get; set; }
}
