namespace KomunalkaAPI.DTO;

public class MeterReadingImageDto
{
    public int Id { get; set; }
    public int ServiceCounterValueId { get; set; }
    public required string OptimizedUrl { get; set; }
    public required string ThumbnailUrl { get; set; }
    public long OptimizedSizeInBytes { get; set; }
    public long ThumbnailSizeInBytes { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public required string MimeType { get; set; }
    public bool IsProcessed { get; set; }
    public DateTime CreatedAt { get; set; }
}
