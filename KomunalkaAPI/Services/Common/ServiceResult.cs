namespace KomunalkaAPI.Services.Common;

public class ServiceResult<T>
{
    public bool Success { get; private set; }
    public bool NotFound { get; private set; }
    public bool Forbidden { get; private set; }
    public List<string> Errors { get; private set; } = new();
    public T? Data { get; private set; }

    public static ServiceResult<T> Ok(T data) => new ServiceResult<T> { Success = true, Data = data };
    public static ServiceResult<T> NotFoundResult(params string[] errors) => new ServiceResult<T> { NotFound = true, Success = false, Data = default, Errors = errors?.ToList() ?? new List<string>() };
    public static ServiceResult<T> ForbiddenResult(params string[] errors) => new ServiceResult<T> { Forbidden = true, Success = false, Data = default, Errors = errors?.ToList() ?? new List<string>() };
    public static ServiceResult<T> Fail(params string[] errors) => new ServiceResult<T> { Success = false, Data = default, Errors = errors?.ToList() ?? new List<string>() };
}
