using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Email.Infrastructure.Entities.Enums;

namespace Email.Infrastructure.Entities;

/// <summary>
/// Represents a cached email message for consolidation functionality
/// </summary>
[Table("EmailCache")]
public class EmailCache
{
    /// <summary>
    /// Unique identifier for the cached email
    /// </summary>
    [Key]
    [Column("Id")]
    public long Id { get; set; }

    /// <summary>
    /// Unique cache key based on hash of To+Cc+Bcc+Subject
    /// </summary>
    [Required]
    [StringLength(500)]
    [Column("CacheKey")]
    public string CacheKey { get; set; } = string.Empty;

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
    /// Consolidated body containing all appended messages
    /// </summary>
    [Required]
    [Column("ConsolidatedBody")]
    public string ConsolidatedBody { get; set; } = string.Empty;

    /// <summary>
    /// Body of the email (for compatibility with services)
    /// </summary>
    [Column("Body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Number of messages consolidated in this cache entry
    /// </summary>
    [Column("MessageCount")]
    public int MessageCount { get; set; } = 1;

    /// <summary>
    /// Timestamp when this cache entry expires
    /// </summary>
    [Required]
    [Column("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    [Column("IsDeleted")]
    public bool IsDeleted { get; set; } = false;

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
}
