namespace DirectPayGateway.Core.DTOs.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public ErrorDetails? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? TraceId { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    public static ApiResponse<T> Fail(string errorCode, string message, string? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ErrorDetails
            {
                Code = errorCode,
                Message = message,
                Details = details
            }
        };
    }
}

public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse Ok(string? message = null)
    {
        return new ApiResponse
        {
            Success = true,
            Message = message
        };
    }

    public new static ApiResponse Fail(string errorCode, string message, string? details = null)
    {
        return new ApiResponse
        {
            Success = false,
            Error = new ErrorDetails
            {
                Code = errorCode,
                Message = message,
                Details = details
            }
        };
    }
}

public class ErrorDetails
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
