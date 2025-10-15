namespace KomunalkaAPI.DTO;

public class ApiResponse<T>
{
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }
    public object? Meta { get; set; }

    public static ApiResponse<T> Success(T? data, string? message = null, object? meta = null)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Message = message,
            Errors = null,
            Meta = meta
        };
    }

    public static ApiResponse<T> Fail(IEnumerable<string> errors, string? message = null, object? meta = null, T? data = default)
    {
        return new ApiResponse<T>
        {
            Data = data,
            Message = message,
            Errors = errors?.ToList() ?? new List<string>(),
            Meta = meta
        };
    }
}