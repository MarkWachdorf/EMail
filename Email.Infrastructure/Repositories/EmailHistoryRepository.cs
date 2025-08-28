using System.Data;
using Dapper;
using Email.Infrastructure.Connection;
using Email.Infrastructure.Entities;
using Email.Infrastructure.Repositories.Interfaces;

namespace Email.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for EmailHistory entity using Dapper.
/// </summary>
public class EmailHistoryRepository : BaseRepository<EmailHistory>, IEmailHistoryRepository
{
    public EmailHistoryRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory, "EmailHistory")
    {
    }

    /// <summary>
    /// Gets history records by email message ID.
    /// </summary>
    public async Task<IEnumerable<EmailHistory>> GetByEmailMessageIdAsync(long emailMessageId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailHistory WHERE EmailMessageId = @EmailMessageId ORDER BY CreatedAt ASC";
        return await connection.QueryAsync<EmailHistory>(sql, new { EmailMessageId = emailMessageId });
    }

    /// <summary>
    /// Gets history records by action.
    /// </summary>
    public async Task<IEnumerable<EmailHistory>> GetByActionAsync(string action)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailHistory WHERE Action = @Action ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<EmailHistory>(sql, new { Action = action });
    }

    /// <summary>
    /// Gets history records by status.
    /// </summary>
    public async Task<IEnumerable<EmailHistory>> GetByStatusAsync(string status)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailHistory WHERE Status = @Status ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<EmailHistory>(sql, new { Status = status });
    }

    /// <summary>
    /// Gets history records by user who performed the action.
    /// </summary>
    public async Task<IEnumerable<EmailHistory>> GetByWhoAsync(string who)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailHistory WHERE Who = @Who ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<EmailHistory>(sql, new { Who = who });
    }

    /// <summary>
    /// Gets history records within a time range.
    /// </summary>
    public async Task<IEnumerable<EmailHistory>> GetByTimeRangeAsync(DateTime startTime, DateTime endTime)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailHistory WHERE CreatedAt BETWEEN @StartTime AND @EndTime ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<EmailHistory>(sql, new { StartTime = startTime, EndTime = endTime });
    }

    /// <summary>
    /// Gets history records by company and application codes.
    /// </summary>
    public async Task<IEnumerable<EmailHistory>> GetByCompanyAndApplicationAsync(string companyCode, string applicationCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT h.* FROM EmailHistory h
            INNER JOIN EmailMessages e ON h.EmailMessageId = e.Id
            WHERE e.CompanyCode = @CompanyCode AND e.ApplicationCode = @ApplicationCode
            ORDER BY h.CreatedAt DESC";

        return await connection.QueryAsync<EmailHistory>(sql, new { CompanyCode = companyCode, ApplicationCode = applicationCode });
    }

    /// <summary>
    /// Gets history statistics by action and time range.
    /// </summary>
    public async Task<object> GetHistoryStatisticsAsync(DateTime startTime, DateTime endTime)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                Action,
                Status,
                COUNT(*) as Count,
                MIN(CreatedAt) as FirstAction,
                MAX(CreatedAt) as LastAction
            FROM EmailHistory
            WHERE CreatedAt BETWEEN @StartTime AND @EndTime
            GROUP BY Action, Status
            ORDER BY Count DESC";

        var results = await connection.QueryAsync(sql, new { StartTime = startTime, EndTime = endTime });
        return results;
    }

    /// <summary>
    /// Cleans up old history records.
    /// </summary>
    public async Task<int> CleanupOldHistoryAsync(DateTime olderThan)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "DELETE FROM EmailHistory WHERE CreatedAt < @OlderThan";
        return await connection.ExecuteAsync(sql, new { OlderThan = olderThan });
    }

    /// <summary>
    /// Gets the SQL for inserting an EmailHistory.
    /// </summary>
    protected override string GetInsertSql()
    {
        return @"
            INSERT INTO EmailHistory (
                EmailMessageId, Action, Status, ErrorMessage, RetryCount, 
                CreatedAt, Who, SentWhen, How, What
            ) VALUES (
                @EmailMessageId, @Action, @Status, @ErrorMessage, @RetryCount, 
                @CreatedAt, @Who, @SentWhen, @How, @What
            );
            SELECT CAST(SCOPE_IDENTITY() as bigint)";
    }

    /// <summary>
    /// Gets the SQL for updating an EmailHistory.
    /// </summary>
    protected override string GetUpdateSql()
    {
        return @"
            UPDATE EmailHistory SET
                EmailMessageId = @EmailMessageId,
                Action = @Action,
                Status = @Status,
                ErrorMessage = @ErrorMessage,
                RetryCount = @RetryCount,
                CreatedAt = @CreatedAt,
                Who = @Who,
                SentWhen = @SentWhen,
                How = @How,
                What = @What
            WHERE Id = @Id";
    }
}
