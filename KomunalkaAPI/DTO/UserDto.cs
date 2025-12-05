namespace KomunalkaAPI.DTO;

public class UserDto
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public required string Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? AvatarThumbnailUrl { get; set; }
    public List<AddressDto>? Addresses { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
