using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class ServiceCounterMeasurement
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Measurement { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}