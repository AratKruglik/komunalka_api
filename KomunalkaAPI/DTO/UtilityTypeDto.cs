namespace KomunalkaAPI.DTO;

public class UtilityTypeDto
{
    public int Id { get; set; }
    public required string Slug { get; set; }
    public required string DisplayName { get; set; }
    public required string Unit { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
