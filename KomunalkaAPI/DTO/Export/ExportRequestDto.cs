using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Export;

public enum ExportFormat
{
    Csv,
    Pdf
}

public class ExportRequestDto
{
    public List<int>? AddressIds { get; set; } = new();

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    public ExportFormat Format { get; set; }
}
