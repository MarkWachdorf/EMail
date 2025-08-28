using Email.Infrastructure.Entities;

namespace Email.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for ErrorLog entity with error logging-specific operations.
/// </summary>
public interface IErrorLogRepository : IBaseRepository<ErrorLog>
{
    /// <summary>
    /// Gets error logs by level.
    /// </summary>
    /// <param name="level">The log level.</param>
    /// <returns>A collection of error logs with the specified level.</returns>
    Task<IEnumerable<ErrorLog>> GetByLevelAsync(string level);

    /// <summary>
    /// Gets error logs by company code.
    /// </summary>
    /// <param name="companyCode">The company code.</param>
    /// <returns>A collection of error logs for the company.</returns>
    Task<IEnumerable<ErrorLog>> GetByCompanyCodeAsync(string companyCode);

    /// <summary>
    /// Gets error logs by request ID for correlation.
    /// </summary>
    /// <param name="requestId">The request ID.</param>
    /// <returns>A collection of error logs for the request.</returns>
    Task<IEnumerable<ErrorLog>> GetByRequestIdAsync(string requestId);

    /// <summary>
    /// Gets error logs by correlation ID for distributed tracing.
    /// </summary>
    /// <param name="correlationId">The correlation ID.</param>
    /// <returns>A collection of error logs for the correlation.</returns>
    Task<IEnumerable<ErrorLog>> GetByCorrelationIdAsync(string correlationId);

    /// <summary>
    /// Gets error logs by email message ID.
    /// </summary>
    /// <param name="emailMessageId">The email message ID.</param>
    /// <returns>A collection of error logs for the email message.</returns>
    Task<IEnumerable<ErrorLog>> GetByEmailMessageIdAsync(long emailMessageId);

    /// <summary>
    /// Gets error logs within a time range.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns>A collection of error logs within the time range.</returns>
    Task<IEnumerable<ErrorLog>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// Gets error statistics by level and time range.
    /// </summary>
    /// <param name="startTime">The start time.</param>
    /// <param name="endTime">The end time.</param>
    /// <returns>Error statistics.</returns>
    Task<object> GetErrorStatisticsAsync(DateTime startTime, DateTime endTime);

    /// <summary>
    /// Cleans up old error logs.
    /// </summary>
    /// <param name="olderThan">Delete logs older than this date.</param>
    /// <returns>The number of logs cleaned up.</returns>
    Task<int> CleanupOldLogsAsync(DateTime olderThan);
}
