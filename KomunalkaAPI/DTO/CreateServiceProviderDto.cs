using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class CreateServiceProviderDto
{
    [Required]
    public int AddressId { get; set; }

    [Required]
    public int UtilityTypeId { get; set; }

    [Required]
    [StringLength(255)]
    public required string Name { get; set; }

    public string? Description { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [Url]
    [StringLength(500)]
    public string? Website { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Tariffs to create with this service provider
    /// </summary>
    public List<CreateTariffDto> Tariffs { get; set; } = new();
}
