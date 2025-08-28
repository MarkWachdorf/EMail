using System.Data;
using Dapper;
using Email.Infrastructure.Connection;
using Email.Infrastructure.Entities;
using Email.Infrastructure.Repositories.Interfaces;

namespace Email.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ErrorLog entity using Dapper.
/// </summary>
public class ErrorLogRepository : BaseRepository<ErrorLog>, IErrorLogRepository
{
    public ErrorLogRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory, "log_errors")
    {
    }

    /// <summary>
    /// Gets error logs by level.
    /// </summary>
    public async Task<IEnumerable<ErrorLog>> GetByLevelAsync(string level)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM log_errors WHERE Level = @Level ORDER BY Timestamp DESC";
        return await connection.QueryAsync<ErrorLog>(sql, new { Level = level });
    }

    /// <summary>
    /// Gets error logs by company code.
    /// </summary>
    public async Task<IEnumerable<ErrorLog>> GetByCompanyCodeAsync(string companyCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM log_errors WHERE CompanyCode = @CompanyCode ORDER BY Timestamp DESC";
        return await connection.QueryAsync<ErrorLog>(sql, new { CompanyCode = companyCode });
    }

    /// <summary>
    /// Gets error logs by request ID for correlation.
    /// </summary>
    public async Task<IEnumerable<ErrorLog>> GetByRequestIdAsync(string requestId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM log_errors WHERE RequestId = @RequestId ORDER BY Timestamp ASC";
        return await connection.QueryAsync<ErrorLog>(sql, new { RequestId = requestId });
    }

    /// <summary>
    /// Gets error logs by correlation ID for distributed tracing.
    /// </summary>
    public async Task<IEnumerable<ErrorLog>> GetByCorrelationIdAsync(string correlationId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM log_errors WHERE CorrelationId = @CorrelationId ORDER BY Timestamp ASC";
        return await connection.QueryAsync<ErrorLog>(sql, new { CorrelationId = correlationId });
    }

    /// <summary>
    /// Gets error logs by email message ID.
    /// </summary>
    public async Task<IEnumerable<ErrorLog>> GetByEmailMessageIdAsync(long emailMessageId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM log_errors WHERE EmailMessageId = @EmailMessageId ORDER BY Timestamp DESC";
        return await connection.QueryAsync<ErrorLog>(sql, new { EmailMessageId = emailMessageId });
    }

    /// <summary>
    /// Gets error logs within a time range.
    /// </summary>
    public async Task<IEnumerable<ErrorLog>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM log_errors WHERE Timestamp BETWEEN @StartTime AND @EndTime ORDER BY Timestamp DESC";
        return await connection.QueryAsync<ErrorLog>(sql, new { StartTime = startTime, EndTime = endTime });
    }

    /// <summary>
    /// Gets error statistics by level and time range.
    /// </summary>
    public async Task<object> GetErrorStatisticsAsync(DateTime startTime, DateTime endTime)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                Level,
                COUNT(*) as Count,
                AVG(CAST(ExecutionTime as float)) as AvgExecutionTime,
                MIN(Timestamp) as FirstError,
                MAX(Timestamp) as LastError
            FROM log_errors
            WHERE Timestamp BETWEEN @StartTime AND @EndTime
            GROUP BY Level
            ORDER BY Count DESC";

        var results = await connection.QueryAsync(sql, new { StartTime = startTime, EndTime = endTime });
        return results;
    }

    /// <summary>
    /// Cleans up old error logs.
    /// </summary>
    public async Task<int> CleanupOldLogsAsync(DateTime olderThan)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "DELETE FROM log_errors WHERE Timestamp < @OlderThan";
        return await connection.ExecuteAsync(sql, new { OlderThan = olderThan });
    }

    /// <summary>
    /// Gets the SQL for inserting an ErrorLog.
    /// </summary>
    protected override string GetInsertSql()
    {
        return @"
            INSERT INTO log_errors (
                Timestamp, Level, Message, Exception, StackTrace, RequestId, CorrelationId, UserId,
                HttpMethod, HttpPath, StatusCode, ExecutionTime, CompanyCode, ApplicationCode, EmailMessageId
            ) VALUES (
                @Timestamp, @Level, @Message, @Exception, @StackTrace, @RequestId, @CorrelationId, @UserId,
                @HttpMethod, @HttpPath, @StatusCode, @ExecutionTime, @CompanyCode, @ApplicationCode, @EmailMessageId
            );
            SELECT CAST(SCOPE_IDENTITY() as bigint)";
    }

    /// <summary>
    /// Gets the SQL for updating an ErrorLog.
    /// </summary>
    protected override string GetUpdateSql()
    {
        return @"
            UPDATE log_errors SET
                Timestamp = @Timestamp,
                Level = @Level,
                Message = @Message,
                Exception = @Exception,
                StackTrace = @StackTrace,
                RequestId = @RequestId,
                CorrelationId = @CorrelationId,
                UserId = @UserId,
                HttpMethod = @HttpMethod,
                HttpPath = @HttpPath,
                StatusCode = @StatusCode,
                ExecutionTime = @ExecutionTime,
                CompanyCode = @CompanyCode,
                ApplicationCode = @ApplicationCode,
                EmailMessageId = @EmailMessageId
            WHERE Id = @Id";
    }
}
