namespace KomunalkaAPI.Models.Responses;

public class ValidationErrorResponse : ErrorResponse
{
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public ValidationErrorResponse()
    {
        Message = "Data validation error";
    }
}
