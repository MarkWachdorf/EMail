using Email.Contracts.Enums;

namespace Email.Contracts.Responses;

/// <summary>
/// Represents an error log response.
/// </summary>
public class ErrorLogResponse
{
    /// <summary>
    /// Unique identifier for the error log entry.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Timestamp when the error occurred.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Log level (Error, Warning, Info, Debug).
    /// </summary>
    public LogLevelDto Level { get; set; }

    /// <summary>
    /// The source of the error (e.g., class name, method name).
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Optional detailed stack trace.
    /// </summary>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Optional additional data in JSON format.
    /// </summary>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Optional company code associated with the error.
    /// </summary>
    public string? CompanyCode { get; set; }

    /// <summary>
    /// Optional application code associated with the error.
    /// </summary>
    public string? ApplicationCode { get; set; }
}
