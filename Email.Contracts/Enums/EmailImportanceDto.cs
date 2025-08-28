namespace Email.Contracts.Enums;

/// <summary>
/// Represents the importance level of an email message for API contracts.
/// </summary>
public enum EmailImportanceDto
{
    /// <summary>
    /// Normal importance.
    /// </summary>
    Normal = 0,

    /// <summary>
    /// High importance.
    /// </summary>
    High = 1,

    /// <summary>
    /// Low importance.
    /// </summary>
    Low = 2
}
