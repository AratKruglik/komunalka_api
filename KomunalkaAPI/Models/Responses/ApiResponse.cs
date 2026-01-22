namespace KomunalkaAPI.Models.Responses;

public class ApiResponse<T>
{
    public T Data { get; set; } = default!;
}
