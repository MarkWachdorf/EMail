using System.ComponentModel.DataAnnotations;
using Email.Contracts.Enums;

namespace Email.Contracts.Requests;

/// <summary>
/// Request to log an error.
/// </summary>
public class LogErrorRequest
{
    /// <summary>
    /// Log level (Error, Warning, Info, Debug).
    /// </summary>
    [Required(ErrorMessage = "Level is required.")]
    public LogLevelDto Level { get; set; }

    /// <summary>
    /// The source of the error (e.g., class name, method name).
    /// </summary>
    [Required(ErrorMessage = "Source is required.")]
    [StringLength(255, ErrorMessage = "Source cannot exceed 255 characters.")]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// The error message.
    /// </summary>
    [Required(ErrorMessage = "Message is required.")]
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
    [StringLength(50, ErrorMessage = "CompanyCode cannot exceed 50 characters.")]
    public string? CompanyCode { get; set; }

    /// <summary>
    /// Optional application code associated with the error.
    /// </summary>
    [StringLength(50, ErrorMessage = "ApplicationCode cannot exceed 50 characters.")]
    public string? ApplicationCode { get; set; }
}
