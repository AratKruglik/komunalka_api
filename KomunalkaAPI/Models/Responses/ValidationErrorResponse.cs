namespace KomunalkaAPI.Models.Responses;

/// <summary>
/// Відповідь про помилки валідації
/// </summary>
public class ValidationErrorResponse : ErrorResponse
{
    /// <summary>
    /// Словник помилок валідації (назва поля -> помилки)
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public ValidationErrorResponse()
    {
        Message = "Помилка валідації даних";
    }
}
