namespace Email.Infrastructure.Entities.Enums;

/// <summary>
/// Represents the status of an email message
/// </summary>
public enum EmailStatus
{
    /// <summary>
    /// Email is pending to be sent
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Email has been successfully sent
    /// </summary>
    Sent = 1,

    /// <summary>
    /// Email failed to send
    /// </summary>
    Failed = 2,

    /// <summary>
    /// Email is cached for consolidation
    /// </summary>
    Cached = 3
}
