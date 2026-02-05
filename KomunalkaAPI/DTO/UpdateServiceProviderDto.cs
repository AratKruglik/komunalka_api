using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class UpdateServiceProviderDto
{
    [StringLength(255)]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [StringLength(50)]
    public string? Phone { get; set; }

    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    [Url]
    [StringLength(500)]
    public string? Website { get; set; }

    public bool? IsActive { get; set; }
}
