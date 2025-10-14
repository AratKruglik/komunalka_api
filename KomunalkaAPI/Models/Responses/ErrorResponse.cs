namespace KomunalkaAPI.Models.Responses;

/// <summary>
/// Standardized error response
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error description (only for Development)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Error timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Request path where the error occurred
    /// </summary>
    public string? Path { get; set; }
}
