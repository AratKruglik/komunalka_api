using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace KomunalkaAPI.DTO;

public class CreateServiceCounterValueDto
{
    [Required]
    public int ServiceCounterId { get; set; }

    [Required]
    [Range(0, float.MaxValue, ErrorMessage = "Value must be greater than or equal to 0")]
    public float Value { get; set; }

    public IFormFile? Image { get; set; }
}
