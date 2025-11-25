using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO;

public class BatchMeterReadingDto
{
    [Required(ErrorMessage = "Address ID is required")]
    public int AddressId { get; set; }

    [Required(ErrorMessage = "At least one reading is required")]
    [MinLength(1, ErrorMessage = "At least one reading is required")]
    public required List<CreateMeterReadingDto> Readings { get; set; }
}
