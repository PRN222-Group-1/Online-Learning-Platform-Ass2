namespace OnlineLearningPlatformAss2.Service.Common;

/// <summary>
/// Generic service result wrapper for operations that return data
/// </summary>
/// <typeparam name="T">The type of data returned</typeparam>
public class ServiceResult<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();

    public ServiceResult()
    {
    }

    public ServiceResult(bool success, string message, T? data = default)
    {
        Success = success;
        Message = message;
        Data = data;
    }

    /// <summary>
    /// Creates a successful result with data
    /// </summary>
    public static ServiceResult<T> SuccessResult(T data, string message = "Operation completed successfully")
    {
        return new ServiceResult<T>(true, message, data);
    }

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static ServiceResult<T> FailureResult(string message)
    {
        return new ServiceResult<T>(false, message);
    }

    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    public static ServiceResult<T> ValidationFailure(Dictionary<string, string[]> errors, string message = "Validation failed")
    {
        return new ServiceResult<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}
