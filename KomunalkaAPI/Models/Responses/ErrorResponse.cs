namespace KomunalkaAPI.Models.Responses;

/// <summary>
/// Стандартизована відповідь про помилку
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP статус код
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Повідомлення про помилку
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Детальний опис помилки (тільки для Development)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Час виникнення помилки
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Шлях запиту, де виникла помилка
    /// </summary>
    public string? Path { get; set; }
}
