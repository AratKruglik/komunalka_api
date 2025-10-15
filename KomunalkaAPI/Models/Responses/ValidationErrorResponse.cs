namespace KomunalkaAPI.Models.Responses;

/// <summary>
/// Validation error response
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Dictionary of validation errors (field name -> errors)
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public ValidationErrorResponse()
    {
        Message = "Data validation error";
    }
}
