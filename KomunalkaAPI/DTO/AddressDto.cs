namespace KomunalkaAPI.DTO;

public class AddressDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ZipCode { get; set; }
    public string City { get; set; }
    public string Street { get; set; }
    public string Building { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}

