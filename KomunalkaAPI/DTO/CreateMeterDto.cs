using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class CreateMeterDto
{
    [Required]
    public int AddressId { get; set; }

    [Required]
    public int UtilityTypeId { get; set; }

    [StringLength(255)]
    public string? SerialNumber { get; set; }

    [Required]
    [StringLength(255)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [StringLength(255)]
    public string? ModelName { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }

    public DateTime? InstallationDate { get; set; }

    public decimal? InitialReading { get; set; }

    public int? ServiceProviderId { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}
