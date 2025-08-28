namespace Email.Infrastructure.Entities.Enums;

/// <summary>
/// Represents the level of a log entry
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Debug level logging
    /// </summary>
    Debug = 0,

    /// <summary>
    /// Information level logging
    /// </summary>
    Info = 1,

    /// <summary>
    /// Warning level logging
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Error level logging
    /// </summary>
    Error = 3
}
