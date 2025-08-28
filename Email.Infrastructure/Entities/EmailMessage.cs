using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Email.Infrastructure.Entities.Enums;

namespace Email.Infrastructure.Entities;

/// <summary>
/// Represents an email message in the system
/// </summary>
[Table("EmailMessages")]
public class EmailMessage
{
    /// <summary>
    /// Unique identifier for the email message
    /// </summary>
    [Key]
    [Column("Id")]
    public long Id { get; set; }

    /// <summary>
    /// Company code to distinguish different customers
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column("CompanyCode")]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// Application code to distinguish which application is sending the email
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column("ApplicationCode")]
    public string ApplicationCode { get; set; } = string.Empty;

    /// <summary>
    /// Sender's email address
    /// </summary>
    [Required]
    [StringLength(255)]
    [Column("FromAddress")]
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Recipient email addresses, separated by semicolons
    /// </summary>
    [Required]
    [Column("ToAddresses")]
    public string ToAddresses { get; set; } = string.Empty;

    /// <summary>
    /// Carbon copy recipient email addresses, separated by semicolons
    /// </summary>
    [Column("CcAddresses")]
    public string? CcAddresses { get; set; }

    /// <summary>
    /// Blind carbon copy recipient email addresses, separated by semicolons
    /// </summary>
    [Column("BccAddresses")]
    public string? BccAddresses { get; set; }

    /// <summary>
    /// Email subject line
    /// </summary>
    [Required]
    [StringLength(500)]
    [Column("Subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Main content of the email
    /// </summary>
    [Required]
    [Column("Body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Header text for the email
    /// </summary>
    [Column("Header")]
    public string? Header { get; set; }

    /// <summary>
    /// Footer text for the email
    /// </summary>
    [Column("Footer")]
    public string? Footer { get; set; }

    /// <summary>
    /// Text used to separate consolidated message bodies
    /// </summary>
    [StringLength(100)]
    [Column("MessageSeparator")]
    public string? MessageSeparator { get; set; }

    /// <summary>
    /// Importance level of the email (Normal, High, Low)
    /// </summary>
    [Required]
    [Column("ImportanceFlag")]
    public EmailImportanceFlag ImportanceFlag { get; set; } = EmailImportanceFlag.Normal;

    /// <summary>
    /// Indicates if the email body is HTML formatted
    /// </summary>
    [Column("HtmlFlag")]
    public bool HtmlFlag { get; set; }

    /// <summary>
    /// If true, an individual message should be sent to each person in To and Cc fields
    /// </summary>
    [Column("SplitFlag")]
    public bool SplitFlag { get; set; }

    /// <summary>
    /// Current status of the email (Pending, Sent, Failed, Cached)
    /// </summary>
    [Required]
    [Column("Status")]
    public EmailStatus Status { get; set; } = EmailStatus.Pending;

    /// <summary>
    /// Error message if the email failed to send
    /// </summary>
    [Column("ErrorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Status message for the email
    /// </summary>
    [Column("StatusMessage")]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// Number of retry attempts made
    /// </summary>
    [Column("RetryCount")]
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Maximum number of retry attempts allowed
    /// </summary>
    [Column("MaxRetries")]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Timestamp when the email was successfully sent
    /// </summary>
    [Column("SentAt")]
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// Timestamp when the email was last attempted to be sent
    /// </summary>
    [Column("LastAttemptedAt")]
    public DateTime? LastAttemptedAt { get; set; }

    /// <summary>
    /// Template code for the email
    /// </summary>
    [StringLength(100)]
    [Column("TemplateCode")]
    public string? TemplateCode { get; set; }

    /// <summary>
    /// Template parameters in JSON format
    /// </summary>
    [Column("TemplateParameters")]
    public string? TemplateParameters { get; set; }

    /// <summary>
    /// Timestamp when the record was created
    /// </summary>
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the record was last updated
    /// </summary>
    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag
    /// </summary>
    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Row version for optimistic concurrency control
    /// </summary>
    [Timestamp]
    [Column("RowVersion")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// User or service that created the record
    /// </summary>
    [StringLength(100)]
    [Column("CreatedBy")]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User or service that last updated the record
    /// </summary>
    [StringLength(100)]
    [Column("UpdatedBy")]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// IP address or service name that created the record
    /// </summary>
    [StringLength(100)]
    [Column("CreatedFrom")]
    public string? CreatedFrom { get; set; }

    /// <summary>
    /// IP address or service name that last updated the record
    /// </summary>
    [StringLength(100)]
    [Column("UpdatedFrom")]
    public string? UpdatedFrom { get; set; }

    /// <summary>
    /// Navigation property for email history
    /// </summary>
    public virtual ICollection<EmailHistory> EmailHistory { get; set; } = new List<EmailHistory>();

    /// <summary>
    /// Navigation property for error logs
    /// </summary>
    public virtual ICollection<ErrorLog> ErrorLogs { get; set; } = new List<ErrorLog>();
}
