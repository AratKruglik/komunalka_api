using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class MeterReadingImage
{
    [Key]
    public int Id { get; set; }

    public int ServiceCounterValueId { get; set; }

    [MaxLength(500)]
    public required string OptimizedPath { get; set; }

    [MaxLength(500)]
    public required string ThumbnailPath { get; set; }

    public long OptimizedSizeInBytes { get; set; }

    public long ThumbnailSizeInBytes { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    [MaxLength(50)]
    public required string MimeType { get; set; }

    public bool IsProcessed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public required ServiceCounterValue ServiceCounterValue { get; set; }
}
