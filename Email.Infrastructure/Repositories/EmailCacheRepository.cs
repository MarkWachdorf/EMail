using System.Data;
using Dapper;
using Email.Infrastructure.Connection;
using Email.Infrastructure.Entities;
using Email.Infrastructure.Repositories.Interfaces;

namespace Email.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for EmailCache entity using Dapper.
/// </summary>
public class EmailCacheRepository : BaseRepository<EmailCache>, IEmailCacheRepository
{
    public EmailCacheRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory, "EmailCache")
    {
    }

    /// <summary>
    /// Gets a cached email by its cache key.
    /// </summary>
    public async Task<EmailCache?> GetByCacheKeyAsync(string cacheKey)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailCache WHERE CacheKey = @CacheKey";
        return await connection.QueryFirstOrDefaultAsync<EmailCache>(sql, new { CacheKey = cacheKey });
    }

    /// <summary>
    /// Gets expired cache entries.
    /// </summary>
    public async Task<IEnumerable<EmailCache>> GetExpiredEntriesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailCache WHERE ExpiresAt < @CurrentTime ORDER BY ExpiresAt ASC";
        return await connection.QueryAsync<EmailCache>(sql, new { CurrentTime = DateTime.UtcNow });
    }

    /// <summary>
    /// Updates the consolidated body and message count for a cached email.
    /// </summary>
    public async Task<bool> UpdateConsolidatedBodyAsync(long id, string consolidatedBody, int messageCount, DateTime expiresAt)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            UPDATE EmailCache 
            SET ConsolidatedBody = @ConsolidatedBody, 
                MessageCount = @MessageCount, 
                ExpiresAt = @ExpiresAt, 
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new 
        { 
            Id = id, 
            ConsolidatedBody = consolidatedBody, 
            MessageCount = messageCount, 
            ExpiresAt = expiresAt, 
            UpdatedAt = DateTime.UtcNow 
        });

        return rowsAffected > 0;
    }

    /// <summary>
    /// Cleans up expired cache entries.
    /// </summary>
    public async Task<int> CleanupExpiredEntriesAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "DELETE FROM EmailCache WHERE ExpiresAt < @CurrentTime";
        return await connection.ExecuteAsync(sql, new { CurrentTime = DateTime.UtcNow });
    }

    /// <summary>
    /// Gets cache entries by company and application codes.
    /// </summary>
    public async Task<IEnumerable<EmailCache>> GetByCompanyAndApplicationAsync(string companyCode, string applicationCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM EmailCache WHERE CompanyCode = @CompanyCode AND ApplicationCode = @ApplicationCode ORDER BY CreatedAt DESC";
        return await connection.QueryAsync<EmailCache>(sql, new { CompanyCode = companyCode, ApplicationCode = applicationCode });
    }

    /// <summary>
    /// Gets the SQL for inserting an EmailCache.
    /// </summary>
    protected override string GetInsertSql()
    {
        return @"
            INSERT INTO EmailCache (
                CacheKey, CompanyCode, ApplicationCode, FromAddress, ToAddresses, CcAddresses, BccAddresses,
                Subject, Header, Footer, MessageSeparator, ImportanceFlag, HtmlFlag, ConsolidatedBody,
                MessageCount, ExpiresAt, CreatedAt, UpdatedAt, CreatedBy, UpdatedBy
            ) VALUES (
                @CacheKey, @CompanyCode, @ApplicationCode, @FromAddress, @ToAddresses, @CcAddresses, @BccAddresses,
                @Subject, @Header, @Footer, @MessageSeparator, @ImportanceFlag, @HtmlFlag, @ConsolidatedBody,
                @MessageCount, @ExpiresAt, @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy
            );
            SELECT CAST(SCOPE_IDENTITY() as bigint)";
    }

    /// <summary>
    /// Gets the SQL for updating an EmailCache.
    /// </summary>
    protected override string GetUpdateSql()
    {
        return @"
            UPDATE EmailCache SET
                CacheKey = @CacheKey,
                CompanyCode = @CompanyCode,
                ApplicationCode = @ApplicationCode,
                FromAddress = @FromAddress,
                ToAddresses = @ToAddresses,
                CcAddresses = @CcAddresses,
                BccAddresses = @BccAddresses,
                Subject = @Subject,
                Header = @Header,
                Footer = @Footer,
                MessageSeparator = @MessageSeparator,
                ImportanceFlag = @ImportanceFlag,
                HtmlFlag = @HtmlFlag,
                ConsolidatedBody = @ConsolidatedBody,
                MessageCount = @MessageCount,
                ExpiresAt = @ExpiresAt,
                UpdatedAt = @UpdatedAt,
                UpdatedBy = @UpdatedBy
            WHERE Id = @Id";
    }
}
