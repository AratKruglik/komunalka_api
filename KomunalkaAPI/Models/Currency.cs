using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models;

public class Currency
{
    [Key]
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Symbol { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}