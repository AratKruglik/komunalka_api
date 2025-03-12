using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class Tariff
{
    [Key]
    public int Id { get; set; }
    public int ServiceCategoryId { get; set; }
    public int ServiceCounterMeasurementId { get; set; }
    public int CurrencyId { get; set; }
    public int AddressId { get; set; }
    public decimal Value { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public required ServiceCategory ServiceCategory { get; set; }
    public required ServiceCounterMeasurement ServiceCounterMeasurement { get; set; }
    public required Currency Currency { get; set; }
}