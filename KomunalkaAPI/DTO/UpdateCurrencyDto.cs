using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class UpdateCurrencyDto
{
    [Required(ErrorMessage = "Currency name is required")]
    [StringLength(50, ErrorMessage = "Currency name cannot exceed 50 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Currency symbol is required")]
    [StringLength(10, ErrorMessage = "Currency symbol cannot exceed 10 characters")]
    public string Symbol { get; set; } = string.Empty;
}
