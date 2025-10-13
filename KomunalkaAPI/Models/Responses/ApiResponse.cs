namespace KomunalkaAPI.Models.Responses;

/// <summary>
/// API response wrapper (Laravel-compatible format)
/// Використовується для wrapping даних у форматі Laravel API Resources
/// </summary>
/// <typeparam name="T">Тип даних</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Основні дані ресурсу
    /// </summary>
    public T Data { get; set; } = default!;
}
