using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class ServiceCounterValue
{
    [Key]
    public int Id { get; set; }
    public int ServiceCounterId { get; set; }
    public float Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public required ServiceCounter ServiceCounter { get; set; }

    // Navigation property for images
    public ICollection<MeterReadingImage> Images { get; set; } = new List<MeterReadingImage>();
}