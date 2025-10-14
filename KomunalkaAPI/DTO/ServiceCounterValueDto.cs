namespace KomunalkaAPI.DTO;

public class ServiceCounterValueDto
{
    public int Id { get; set; }
    public int ServiceCounterId { get; set; }
    public float Value { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<MeterReadingImageDto> Images { get; set; } = new();
}
