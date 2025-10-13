using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class CreateAddressDto
{
    [Required(ErrorMessage = "Region is required")]
    public int RegionId { get; set; }

    [Required(ErrorMessage = "City is required")]
    [StringLength(100, ErrorMessage = "City name cannot exceed 100 characters")]
    public required string City { get; set; }

    [Required(ErrorMessage = "Street is required")]
    [StringLength(200, ErrorMessage = "Street name cannot exceed 200 characters")]
    public required string Street { get; set; }

    [Required(ErrorMessage = "Building number is required")]
    [StringLength(20, ErrorMessage = "Building number cannot exceed 20 characters")]
    public required string BuildingNumber { get; set; }

    [StringLength(20, ErrorMessage = "Apartment number cannot exceed 20 characters")]
    public string? ApartmentNumber { get; set; }

    [Required(ErrorMessage = "Zip code is required")]
    [StringLength(5, MinimumLength = 5, ErrorMessage = "Zip code must be exactly 5 characters")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "Zip code must contain only 5 digits")]
    public required string ZipCode { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }

    public bool IsPrimary { get; set; }

    [Required(ErrorMessage = "Address type is required")]
    public int AddressTypeId { get; set; }
}