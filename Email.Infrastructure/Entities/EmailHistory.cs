using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Email.Infrastructure.Entities.Enums;

namespace Email.Infrastructure.Entities;

/// <summary>
/// Represents the audit history of email message operations
/// </summary>
[Table("EmailHistory")]
public class EmailHistory
{
    /// <summary>
    /// Unique identifier for the history record
    /// </summary>
    [Key]
    [Column("Id")]
    public long Id { get; set; }

    /// <summary>
    /// Foreign key to the email message
    /// </summary>
    [Required]
    [Column("EmailMessageId")]
    public long EmailMessageId { get; set; }

    /// <summary>
    /// Action performed on the email (Created, Sent, Failed, Retried, Cached, Deleted)
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column("Action")]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// Status of the email at the time of the action
    /// </summary>
    [Required]
    [Column("Status")]
    public EmailStatus Status { get; set; } = EmailStatus.Pending;

    /// <summary>
    /// Error message if the action failed
    /// </summary>
    [Column("ErrorMessage")]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Details of the action performed
    /// </summary>
    [Column("Details")]
    public string? Details { get; set; }

    /// <summary>
    /// Retry count at the time of the action
    /// </summary>
    [Column("RetryCount")]
    public int? RetryCount { get; set; }

    /// <summary>
    /// Timestamp when the history record was created
    /// </summary>
    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User or service that performed the action (API Standards: who)
    /// </summary>
    [StringLength(100)]
    [Column("Who")]
    public string? Who { get; set; }

    /// <summary>
    /// Timestamp when the action was performed (API Standards: when)
    /// </summary>
    [Column("SentWhen")]
    public DateTime SentWhen { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Method or endpoint used to perform the action (API Standards: how)
    /// </summary>
    [StringLength(100)]
    [Column("How")]
    public string? How { get; set; }

    /// <summary>
    /// Description of what was done (API Standards: what)
    /// </summary>
    [Column("What")]
    public string? What { get; set; }

    /// <summary>
    /// Navigation property to the email message
    /// </summary>
    public virtual EmailMessage EmailMessage { get; set; } = null!;
}
