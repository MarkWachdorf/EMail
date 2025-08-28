namespace Email.Infrastructure.Entities.Enums;

/// <summary>
/// Represents the importance level of an email message
/// </summary>
public enum EmailImportanceFlag
{
    /// <summary>
    /// Normal importance
    /// </summary>
    Normal = 0,

    /// <summary>
    /// High importance
    /// </summary>
    High = 1,

    /// <summary>
    /// Low importance
    /// </summary>
    Low = 2
}
