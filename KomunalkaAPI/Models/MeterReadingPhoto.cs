using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

/// <summary>
/// Photo attached to meter reading
/// </summary>
[Table("meter_reading_photos")]
public class MeterReadingPhoto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("meter_reading_id")]
    public int MeterReadingId { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("optimized_path")]
    public required string OptimizedPath { get; set; }

    [Required]
    [MaxLength(500)]
    [Column("thumbnail_path")]
    public required string ThumbnailPath { get; set; }

    [Column("optimized_size_in_bytes")]
    public long OptimizedSizeInBytes { get; set; }

    [Column("thumbnail_size_in_bytes")]
    public long ThumbnailSizeInBytes { get; set; }

    [Column("width")]
    public int Width { get; set; }

    [Column("height")]
    public int Height { get; set; }

    [MaxLength(50)]
    [Column("mime_type")]
    public required string MimeType { get; set; }

    [Column("is_processed")]
    public bool IsProcessed { get; set; } = false;

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(MeterReadingId))]
    public virtual MeterReading MeterReading { get; set; } = null!;
}
