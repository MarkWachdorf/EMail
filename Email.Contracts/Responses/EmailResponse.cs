using Email.Contracts.Enums;

namespace Email.Contracts.Responses;

/// <summary>
/// Represents an email message response.
/// </summary>
public class EmailResponse
{
    /// <summary>
    /// Unique identifier for the email message.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Company code to distinguish different customers.
    /// </summary>
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// Application code to distinguish which application is sending the email.
    /// </summary>
    public string ApplicationCode { get; set; } = string.Empty;

    /// <summary>
    /// The sender's email address.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of recipient email addresses.
    /// </summary>
    public string ToAddresses { get; set; } = string.Empty;

    /// <summary>
    /// Optional comma-separated list of CC recipient email addresses.
    /// </summary>
    public string? CcAddresses { get; set; }

    /// <summary>
    /// Optional comma-separated list of BCC recipient email addresses.
    /// </summary>
    public string? BccAddresses { get; set; }

    /// <summary>
    /// The subject of the email.
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// The body of the email (HTML or plain text).
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Optional header content for the email.
    /// </summary>
    public string? Header { get; set; }

    /// <summary>
    /// Optional footer content for the email.
    /// </summary>
    public string? Footer { get; set; }

    /// <summary>
    /// Current status of the email (Pending, Sent, Failed, Cached).
    /// </summary>
    public EmailStatusDto Status { get; set; }

    /// <summary>
    /// Optional message providing details about the current status.
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Importance level of the email.
    /// </summary>
    public EmailImportanceDto ImportanceFlag { get; set; }

    /// <summary>
    /// Number of times the email has been attempted to send.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Maximum number of retry attempts for sending the email.
    /// </summary>
    public int MaxRetries { get; set; }

    /// <summary>
    /// Timestamp of the last attempt to send the email.
    /// </summary>
    public DateTime? LastAttemptedAt { get; set; }

    /// <summary>
    /// Optional template code used for the email.
    /// </summary>
    public string? TemplateCode { get; set; }

    /// <summary>
    /// Optional JSON string for template parameters.
    /// </summary>
    public string? TemplateParameters { get; set; }

    /// <summary>
    /// Timestamp when the record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Row version for optimistic concurrency.
    /// </summary>
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
