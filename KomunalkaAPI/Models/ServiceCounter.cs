using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class ServiceCounter
{
    [Key]
    public int Id { get; set; }
    public int AddressId { get; set; }
    public int ServiceCategoryId { get; set; }
    public string? SerialNumber { get; set; }
    public int ServiceCounterMeasurementId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public ServiceCounterMeasurement ServiceCounterMeasurement { get; set; }
    public ServiceCategory ServiceCategory { get; set; }
    
}