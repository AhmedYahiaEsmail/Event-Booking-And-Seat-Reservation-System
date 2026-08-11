namespace EventBooking.API.Common;

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }

    public ApiResponse() { }

    public ApiResponse(bool isSuccess, string message, T? data, List<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Message = message;
        Data = data;
        Errors = errors;
    }

    public static ApiResponse<T> Success(T data, string message = "Success")
    {
        return new ApiResponse<T>(true, message, data);
    }

    public static ApiResponse<T> Failure(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>(false, message, default, errors);
    }
}
