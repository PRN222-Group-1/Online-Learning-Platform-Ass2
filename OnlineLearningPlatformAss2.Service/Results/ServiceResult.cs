namespace OnlineLearningPlatformAss2.Service.Results;

public class ServiceResult<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public string? Message { get; init; }
    public List<string> Errors { get; init; } = [];

    public static ServiceResult<T> SuccessResult(T data, string? message = "Success") => new()
    {
        Success = true,
        Data = data,
        Message = message
    };

    public static ServiceResult<T> FailureResult(string message) => new()
    {
        Success = false,
        Message = message,
        Errors = [message]
    };

    public static ServiceResult<T> FailureResult(List<string> errors) => new()
    {
        Success = false,
        Errors = errors,
        Message = errors is { Count: > 0 }
            ? string.Join(" | ", errors)
            : "Operation failed"
    };
}
