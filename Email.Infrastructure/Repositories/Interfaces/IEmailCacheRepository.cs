using Email.Infrastructure.Entities;

namespace Email.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for EmailCache entity with caching-specific operations.
/// </summary>
public interface IEmailCacheRepository : IBaseRepository<EmailCache>
{
    /// <summary>
    /// Gets a cached email by its cache key.
    /// </summary>
    /// <param name="cacheKey">The cache key.</param>
    /// <returns>The cached email if found, null otherwise.</returns>
    Task<EmailCache?> GetByCacheKeyAsync(string cacheKey);

    /// <summary>
    /// Gets expired cache entries.
    /// </summary>
    /// <returns>A collection of expired cache entries.</returns>
    Task<IEnumerable<EmailCache>> GetExpiredEntriesAsync();

    /// <summary>
    /// Updates the consolidated body and message count for a cached email.
    /// </summary>
    /// <param name="id">The cache entry ID.</param>
    /// <param name="consolidatedBody">The new consolidated body.</param>
    /// <param name="messageCount">The new message count.</param>
    /// <param name="expiresAt">The new expiration time.</param>
    /// <returns>True if updated successfully, false otherwise.</returns>
    Task<bool> UpdateConsolidatedBodyAsync(long id, string consolidatedBody, int messageCount, DateTime expiresAt);

    /// <summary>
    /// Cleans up expired cache entries.
    /// </summary>
    /// <returns>The number of entries cleaned up.</returns>
    Task<int> CleanupExpiredEntriesAsync();

    /// <summary>
    /// Gets cache entries by company and application codes.
    /// </summary>
    /// <param name="companyCode">The company code.</param>
    /// <param name="applicationCode">The application code.</param>
    /// <returns>A collection of cache entries.</returns>
    Task<IEnumerable<EmailCache>> GetByCompanyAndApplicationAsync(string companyCode, string applicationCode);
}
