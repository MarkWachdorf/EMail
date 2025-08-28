using System.Data;
using Dapper;
using Email.Infrastructure.Connection;
using Email.Infrastructure.Entities;
using Email.Infrastructure.Repositories.Interfaces;

namespace Email.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for EmailMessage entity using Dapper.
/// </summary>
public class EmailMessageRepository : BaseRepository<EmailMessage>, IEmailMessageRepository
{
    public EmailMessageRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory, "EmailMessages")
    {
    }

    /// <summary>
    /// Gets unsent messages for a company and application.
    /// </summary>
    public async Task<IEnumerable<EmailMessage>> GetUnsentMessagesAsync(string companyCode, string? applicationCode = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            SELECT * FROM EmailMessages 
            WHERE CompanyCode = @CompanyCode 
                AND (@ApplicationCode IS NULL OR ApplicationCode LIKE @ApplicationCode)
                AND Status IN ('Failed', 'Pending')
                AND IsDeleted = 0 
                AND RetryCount < MaxRetries
            ORDER BY CreatedAt ASC";

        return await connection.QueryAsync<EmailMessage>(sql, new { CompanyCode = companyCode, ApplicationCode = applicationCode });
    }

    /// <summary>
    /// Gets messages by status.
    /// </summary>
    public async Task<IEnumerable<EmailMessage>> GetByStatusAsync(string status)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailMessages WHERE Status = @Status AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<EmailMessage>(sql, new { Status = status });
    }

    /// <summary>
    /// Gets messages by company and application codes.
    /// </summary>
    public async Task<IEnumerable<EmailMessage>> GetByCompanyAndApplicationAsync(string companyCode, string applicationCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailMessages WHERE CompanyCode = @CompanyCode AND ApplicationCode = @ApplicationCode AND IsDeleted = 0 ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<EmailMessage>(sql, new { CompanyCode = companyCode, ApplicationCode = applicationCode });
    }

    /// <summary>
    /// Updates the status of an email message.
    /// </summary>
    public async Task<bool> UpdateStatusAsync(long id, string status, string? errorMessage = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            UPDATE EmailMessages 
            SET Status = @Status, 
                ErrorMessage = @ErrorMessage, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND IsDeleted = 0";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            Status = status, 
            ErrorMessage = errorMessage, 
            UpdatedAt = DateTime.UtcNow 
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Increments the retry count for an email message.
    /// </summary>
    public async Task<bool> IncrementRetryCountAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            UPDATE EmailMessages 
            SET RetryCount = RetryCount + 1, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND IsDeleted = 0";

        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    /// <summary>
    /// Marks an email as sent.
    /// </summary>
    public async Task<bool> MarkAsSentAsync(long id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            UPDATE EmailMessages 
            SET Status = 'Sent', 
                SentAt = @SentAt, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id AND IsDeleted = 0";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            SentAt = DateTime.UtcNow, 
            UpdatedAt = DateTime.UtcNow 
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Gets failed messages that can be retried.
    /// </summary>
    public async Task<IEnumerable<EmailMessage>> GetFailedMessagesForRetryAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT * FROM EmailMessages 
            WHERE Status = 'Failed' 
                AND IsDeleted = 0 
                AND RetryCount < MaxRetries
            ORDER BY CreatedAt ASC";

        return await connection.QueryAsync<EmailMessage>(sql);
    }

    /// <summary>
    /// Gets email statistics by company and application.
    /// </summary>
    public async Task<object> GetEmailStatisticsAsync(string companyCode, string? applicationCode = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                Status,
                COUNT(*) as Count,
                MIN(CreatedAt) as FirstEmail,
                MAX(CreatedAt) as LastEmail
            FROM EmailMessages
            WHERE CompanyCode = @CompanyCode 
                AND (@ApplicationCode IS NULL OR ApplicationCode LIKE @ApplicationCode)
                AND IsDeleted = 0
            GROUP BY Status";

        var results = await connection.QueryAsync(sql, new { CompanyCode = companyCode, ApplicationCode = applicationCode });
        return results;
    }

    /// <summary>
    /// Gets the SQL for inserting an EmailMessage.
    /// </summary>
    protected override string GetInsertSql()
    {
        return @"
            INSERT INTO EmailMessages (
                CompanyCode, ApplicationCode, FromAddress, ToAddresses, CcAddresses, BccAddresses,
                Subject, Body, Header, Footer, MessageSeparator, ImportanceFlag, HtmlFlag, SplitFlag,
                Status, ErrorMessage, RetryCount, MaxRetries, SentAt, CreatedAt, UpdatedAt, IsDeleted,
                CreatedBy, UpdatedBy, CreatedFrom, UpdatedFrom
            ) VALUES (
                @CompanyCode, @ApplicationCode, @FromAddress, @ToAddresses, @CcAddresses, @BccAddresses,
                @Subject, @Body, @Header, @Footer, @MessageSeparator, @ImportanceFlag, @HtmlFlag, @SplitFlag,
                @Status, @ErrorMessage, @RetryCount, @MaxRetries, @SentAt, @CreatedAt, @UpdatedAt, @IsDeleted,
                @CreatedBy, @UpdatedBy, @CreatedFrom, @UpdatedFrom
            );
            SELECT CAST(SCOPE_IDENTITY() as bigint)";
    }

    /// <summary>
    /// Gets the SQL for updating an EmailMessage.
    /// </summary>
    protected override string GetUpdateSql()
    {
        return @"
            UPDATE EmailMessages SET
                CompanyCode = @CompanyCode,
                ApplicationCode = @ApplicationCode,
                FromAddress = @FromAddress,
                ToAddresses = @ToAddresses,
                CcAddresses = @CcAddresses,
                BccAddresses = @BccAddresses,
                Subject = @Subject,
                Body = @Body,
                Header = @Header,
                Footer = @Footer,
                MessageSeparator = @MessageSeparator,
                ImportanceFlag = @ImportanceFlag,
                HtmlFlag = @HtmlFlag,
                SplitFlag = @SplitFlag,
                Status = @Status,
                ErrorMessage = @ErrorMessage,
                RetryCount = @RetryCount,
                MaxRetries = @MaxRetries,
                SentAt = @SentAt,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy,
                UpdatedFrom = @UpdatedFrom
            WHERE Id = @Id AND IsDeleted = 0";
    }
}
