using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class UpdateCurrencyDto
{
    [Required(ErrorMessage = "Currency code is required")]
    [StringLength(10, ErrorMessage = "Currency code cannot exceed 10 characters")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Currency name is required")]
    [StringLength(255, ErrorMessage = "Currency name cannot exceed 255 characters")]
    public string Name { get; set; } = string.Empty;

    [StringLength(10, ErrorMessage = "Currency symbol cannot exceed 10 characters")]
    public string? Symbol { get; set; }
}
