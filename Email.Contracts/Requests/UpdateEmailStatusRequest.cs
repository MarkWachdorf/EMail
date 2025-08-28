using System.ComponentModel.DataAnnotations;
using Email.Contracts.Enums;

namespace Email.Contracts.Requests;

/// <summary>
/// Request to update the status of an email message.
/// </summary>
public class UpdateEmailStatusRequest
{
    /// <summary>
    /// Unique identifier for the email message.
    /// </summary>
    [Required(ErrorMessage = "EmailId is required.")]
    [Range(1, long.MaxValue, ErrorMessage = "EmailId must be a positive value.")]
    public long EmailId { get; set; }

    /// <summary>
    /// The new status for the email message.
    /// </summary>
    [Required(ErrorMessage = "NewStatus is required.")]
    public EmailStatusDto NewStatus { get; set; }

    /// <summary>
    /// Optional message providing details about the status update (e.g., error message).
    /// </summary>
    public string? StatusMessage { get; set; }

    /// <summary>
    /// The current row version for optimistic concurrency.
    /// </summary>
    [Required(ErrorMessage = "RowVersion is required for optimistic concurrency.")]
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
