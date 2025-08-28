using Email.Infrastructure.Entities;

namespace Email.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for EmailHistory entity with history tracking-specific operations.
/// </summary>
public interface IEmailHistoryRepository : IBaseRepository<EmailHistory>
{
    /// <summary>
    /// Gets history records by email message ID.
    /// </summary>
    /// <param name="emailMessageId">The email message ID.</param>
    /// <returns>A collection of history records for the email message.</returns>
    Task<IEnumerable<EmailHistory>> GetByEmailMessageIdAsync(long emailMessageId);

    /// <summary>
    /// Gets history records by action.
    /// </summary>
    /// <param name="action">The action performed.</param>
    /// <returns>A collection of history records for the action.</returns>
    Task<IEnumerable<EmailHistory>> GetByActionAsync(string action);

    /// <summary>
    /// Gets history records by status.
    /// </summary>
    /// <param name="status">The email status.</param>
    /// <returns>A collection of history records for the status.</returns>
    Task<IEnumerable<EmailHistory>> GetByStatusAsync(string status);

    /// <summary>
    /// Gets history records by user who performed the action.
    /// </summary>
    /// <param name="who">The user or service that performed the action.</param>
    /// <returns>A collection of history records for the user.</returns>
    Task<IEnumerable<EmailHistory>> GetByWhoAsync(string who);

    /// <summary>
    /// Gets history records within a time range.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns>A collection of history records within the time range.</returns>
    Task<IEnumerable<EmailHistory>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// Gets history records by company and application codes.
    /// </summary>
    /// <param name="companyCode">The company code.</param>
    /// <param name="applicationCode">The application code.</param>
    /// <returns>A collection of history records for the company and application.</returns>
    Task<IEnumerable<EmailHistory>> GetByCompanyAndApplicationAsync(string companyCode, string applicationCode);

    /// <summary>
    /// Gets history statistics by action and time range.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns>History statistics.</returns>
    Task<object> GetHistoryStatisticsAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// Cleans up old history records.
    /// </summary>
    /// <param name="olderThan">Delete records older than this date.</param>
    /// <returns>The number of records cleaned up.</returns>
    Task<int> CleanupOldHistoryAsync(DateTime olderThan);
}
