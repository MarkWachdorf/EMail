namespace Email.Contracts.Responses;

/// <summary>
/// Base response class for standardized API responses.
/// </summary>
/// <typeparam name="T">The type of the data payload.</typeparam>
public class BaseResponse<T>
{
    /// <summary>
    /// Indicates if the operation was successful.
    /// </summary>
    public bool Succeeded { get; set; }

    /// <summary>
    /// The data payload of the response.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// A message providing additional information about the response.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// A list of errors if the operation failed.
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Creates a successful response.
    /// </summary>
    /// <param name="data">The data payload.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful BaseResponse.</returns>
    public static BaseResponse<T> Success(T data, string? message = null)
    {
        return new BaseResponse<T> { Succeeded = true, Data = data, Message = message };
    }

    /// <summary>
    /// Creates a failed response.
    /// </summary>
    /// <param name="errors">A list of error messages.</param>
    /// <param name="message">Optional general error message.</param>
    /// <returns>A failed BaseResponse.</returns>
    public static BaseResponse<T> Fail(List<string> errors, string? message = "Operation failed.")
    {
        return new BaseResponse<T> { Succeeded = false, Errors = errors, Message = message };
    }

    /// <summary>
    /// Creates a failed response with a single error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <param name="message">Optional general error message.</param>
    /// <returns>A failed BaseResponse.</returns>
    public static BaseResponse<T> Fail(string error, string? message = "Operation failed.")
    {
        return new BaseResponse<T> { Succeeded = false, Errors = new List<string> { error }, Message = message };
    }
}
