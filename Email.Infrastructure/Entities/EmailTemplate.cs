using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Email.Infrastructure.Entities;

/// <summary>
/// Represents an email template for reusable email content
/// </summary>
[Table("EmailTemplates")]
public class EmailTemplate
{
    /// <summary>
    /// Unique identifier for the email template
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
    /// Application code to distinguish which application uses this template
    /// </summary>
    [Required]
    [StringLength(50)]
    [Column("ApplicationCode")]
    public string ApplicationCode { get; set; } = string.Empty;

    /// <summary>
    /// Name of the template
    /// </summary>
    [Required]
    [StringLength(100)]
    [Column("TemplateName")]
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Email subject line template
    /// </summary>
    [Required]
    [StringLength(500)]
    [Column("Subject")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body template
    /// </summary>
    [Required]
    [Column("Body")]
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Header text template
    /// </summary>
    [Column("Header")]
    public string? Header { get; set; }

    /// <summary>
    /// Footer text template
    /// </summary>
    [Column("Footer")]
    public string? Footer { get; set; }

    /// <summary>
    /// Indicates if the template is active and available for use
    /// </summary>
    [Column("IsActive")]
    public bool IsActive { get; set; } = true;

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
}
