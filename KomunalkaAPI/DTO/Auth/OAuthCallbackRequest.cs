using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.DTO.Auth;

public class OAuthCallbackRequest
{
    [Required(ErrorMessage = "Provider is required")]
    public required string Provider { get; set; }

    public string? Code { get; set; }
    public string? State { get; set; }
    public string? Error { get; set; }
    public string? ErrorDescription { get; set; }
}
