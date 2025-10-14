namespace KomunalkaAPI.Models.Responses;

/// <summary>
/// API response wrapper (Laravel-compatible format)
/// Used for wrapping data in Laravel API Resources format
/// </summary>
/// <typeparam name="T">Data type</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Resource data
    /// </summary>
    public T Data { get; set; } = default!;
}
