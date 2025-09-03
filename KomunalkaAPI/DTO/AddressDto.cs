using KomunalkaAPI.Models;

namespace KomunalkaAPI.DTO;

public class AddressDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RegionId { get; set; }
    public required string City { get; set; }
    public required string Street { get; set; }
    public required string BuildingNumber { get; set; }
    public string? ApartmentNumber { get; set; }
    public required string ZipCode { get; set; }
    public string? Notes { get; set; }
    public bool IsPrimary { get; set; }
    public int AddressTypeId { get; set; }
    // public User? User { get; set; }
    public RegionDto? Region { get; set; }
    public AddressType? AddressType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

