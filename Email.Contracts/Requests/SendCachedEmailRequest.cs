using System.ComponentModel.DataAnnotations;
using Email.Contracts.Enums;

namespace Email.Contracts.Requests;

/// <summary>
/// Request to send an email that might be cached for consolidation.
/// </summary>
public class SendCachedEmailRequest
{
    /// <summary>
    /// Company code to distinguish different customers.
    /// </summary>
    [Required(ErrorMessage = "CompanyCode is required.")]
    [StringLength(50, ErrorMessage = "CompanyCode cannot exceed 50 characters.")]
    public string CompanyCode { get; set; } = string.Empty;

    /// <summary>
    /// Application code to distinguish which application is sending the email.
    /// </summary>
    [Required(ErrorMessage = "ApplicationCode is required.")]
    [StringLength(50, ErrorMessage = "ApplicationCode cannot exceed 50 characters.")]
    public string ApplicationCode { get; set; } = string.Empty;

    /// <summary>
    /// The sender's email address.
    /// </summary>
    [Required(ErrorMessage = "FromAddress is required.")]
    [EmailAddress(ErrorMessage = "Invalid FromAddress format.")]
    [StringLength(255, ErrorMessage = "FromAddress cannot exceed 255 characters.")]
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Comma-separated list of recipient email addresses.
    /// </summary>
    [Required(ErrorMessage = "ToAddresses is required.")]
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
    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(500, ErrorMessage = "Subject cannot exceed 500 characters.")]
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// The body of the email (HTML or plain text).
    /// </summary>
    [Required(ErrorMessage = "Body is required.")]
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
    /// Importance level of the email.
    /// </summary>
    public EmailImportanceDto ImportanceFlag { get; set; } = EmailImportanceDto.Normal;

    /// <summary>
    /// Maximum number of retry attempts for sending the email.
    /// </summary>
    [Range(0, 10, ErrorMessage = "MaxRetries must be between 0 and 10.")]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Optional template code to use for the email.
    /// </summary>
    [StringLength(100, ErrorMessage = "TemplateCode cannot exceed 100 characters.")]
    public string? TemplateCode { get; set; }

    /// <summary>
    /// Optional JSON string for template parameters.
    /// </summary>
    public string? TemplateParameters { get; set; }

    /// <summary>
    /// Expiration time for the cached email in minutes.
    /// </summary>
    [Range(1, 1440, ErrorMessage = "CacheExpirationMinutes must be between 1 and 1440 (24 hours).")]
    public int CacheExpirationMinutes { get; set; } = 60;
}
