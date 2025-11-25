namespace KomunalkaAPI.DTO;

public class MeterReadingPhotoDto
{
    public int Id { get; set; }
    public string? OptimizedUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsProcessed { get; set; }
}
