namespace KomunalkaAPI.DTO;

public class ServiceProviderDto
{
    public int Id { get; set; }
    public int AddressId { get; set; }
    public int UtilityTypeId { get; set; }
    public string? UtilityTypeName { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
