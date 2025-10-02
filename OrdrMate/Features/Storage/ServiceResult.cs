namespace OrdrMate.Features.Storage;

public class ServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public int StatusCode { get; set; } = 200;

    public static ServiceResult<T> Success(T data)
    {
        return new ServiceResult<T>
        {
            IsSuccess = true,
            Data = data,
            StatusCode = 200
        };
    }

    public static ServiceResult<T> Error(string message, int statusCode = 400)
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = statusCode
        };
    }

    public static ServiceResult<T> NotFound(string message = "Resource not found")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = 404
        };
    }

    public static ServiceResult<T> InternalError(string message = "Internal server error")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = 500
        };
    }

    public static ServiceResult<T> Forbidden(string message = "Access forbidden")
    {
        return new ServiceResult<T>
        {
            IsSuccess = false,
            ErrorMessage = message,
            StatusCode = 403
        };
    }
}