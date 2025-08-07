using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KomunalkaAPI.Models;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }

    public required string Token { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsUsed { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("User")]
    public int UserId { get; set; }

    public User? User { get; set; }
}
