using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Email.Infrastructure.Entities.Enums;

namespace Email.Infrastructure.Entities;

/// <summary>
/// Represents an error log entry for structured logging
/// </summary>
[Table("log_errors")]
public class ErrorLog
{
    /// <summary>
    /// Unique identifier for the error log entry
    /// </summary>
    [Key]
    [Column("Id")]
    public long Id { get; set; }

    /// <summary>
    /// Timestamp when the error occurred
    /// </summary>
    [Required]
    [Column("Timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Log level (Error, Warning, Info, Debug)
    /// </summary>
    [Required]
    [Column("Level")]
    public LogLevel Level { get; set; } = LogLevel.Error;

    /// <summary>
    /// The source of the error (e.g., class name, method name)
    /// </summary>
    [Required]
    [StringLength(255)]
    [Column("Source")]
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Error message
    /// </summary>
    [Required]
    [Column("Message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Exception details
    /// </summary>
    [Column("Exception")]
    public string? Exception { get; set; }

    /// <summary>
    /// Stack trace information
    /// </summary>
    [Column("StackTrace")]
    public string? StackTrace { get; set; }

    /// <summary>
    /// Additional data in JSON format
    /// </summary>
    [Column("AdditionalData")]
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Request identifier for correlation
    /// </summary>
    [StringLength(100)]
    [Column("RequestId")]
    public string? RequestId { get; set; }

    /// <summary>
    /// Correlation identifier for distributed tracing
    /// </summary>
    [StringLength(100)]
    [Column("CorrelationId")]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// User identifier who triggered the error
    /// </summary>
    [StringLength(100)]
    [Column("UserId")]
    public string? UserId { get; set; }

    /// <summary>
    /// HTTP method that caused the error
    /// </summary>
    [StringLength(10)]
    [Column("HttpMethod")]
    public string? HttpMethod { get; set; }

    /// <summary>
    /// HTTP path that caused the error
    /// </summary>
    [StringLength(500)]
    [Column("HttpPath")]
    public string? HttpPath { get; set; }

    /// <summary>
    /// HTTP status code returned
    /// </summary>
    [Column("StatusCode")]
    public int? StatusCode { get; set; }

    /// <summary>
    /// Execution time in milliseconds
    /// </summary>
    [Column("ExecutionTime")]
    public int? ExecutionTime { get; set; }

    /// <summary>
    /// Company code associated with the error
    /// </summary>
    [StringLength(50)]
    [Column("CompanyCode")]
    public string? CompanyCode { get; set; }

    /// <summary>
    /// Application code associated with the error
    /// </summary>
    [StringLength(50)]
    [Column("ApplicationCode")]
    public string? ApplicationCode { get; set; }

    /// <summary>
    /// Foreign key to the email message if error is related to email processing
    /// </summary>
    [Column("EmailMessageId")]
    public long? EmailMessageId { get; set; }

    /// <summary>
    /// Navigation property to the email message
    /// </summary>
    public virtual EmailMessage? EmailMessage { get; set; }
}
