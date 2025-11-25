namespace KomunalkaAPI.DTO;

public class MeterDto
{
    public int Id { get; set; }
    public int AddressId { get; set; }
    public int UtilityTypeId { get; set; }
    public string? SerialNumber { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? ModelName { get; set; }
    public string? Location { get; set; }
    public string? PhotoPath { get; set; }
    public DateTime? InstallationDate { get; set; }
    public decimal? InitialReading { get; set; }
    public int? ServiceProviderId { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties (optional)
    public string? UtilityTypeName { get; set; }
    public string? ServiceProviderName { get; set; }
}
