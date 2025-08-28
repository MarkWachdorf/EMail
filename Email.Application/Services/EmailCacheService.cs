using System.Security.Cryptography;
using System.Text;
using Email.Application.Mappers;
using Email.Application.Services.Interfaces;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Email.Contracts.Enums;
using Email.Infrastructure.Entities;
using Email.Infrastructure.Entities.Enums;
using Email.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Email.Application.Services;

/// <summary>
/// Service for managing cached email messages for consolidation.
/// </summary>
public class EmailCacheService : IEmailCacheService
{
    private readonly IEmailCacheRepository _emailCacheRepository;
    private readonly IEmailMessageRepository _emailMessageRepository;
    private readonly IEmailHistoryRepository _emailHistoryRepository;
    private readonly IEmailSenderService _emailSenderService;
    private readonly ILogger<EmailCacheService> _logger;

    public EmailCacheService(
        IEmailCacheRepository emailCacheRepository,
        IEmailMessageRepository emailMessageRepository,
        IEmailHistoryRepository emailHistoryRepository,
        IEmailSenderService emailSenderService,
        ILogger<EmailCacheService> logger)
    {
        _emailCacheRepository = emailCacheRepository ?? throw new ArgumentNullException(nameof(emailCacheRepository));
        _emailMessageRepository = emailMessageRepository ?? throw new ArgumentNullException(nameof(emailMessageRepository));
        _emailHistoryRepository = emailHistoryRepository ?? throw new ArgumentNullException(nameof(emailHistoryRepository));
        _emailSenderService = emailSenderService ?? throw new ArgumentNullException(nameof(emailSenderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<EmailResponse> SendCachedEmailAsync(SendCachedEmailRequest request)
    {
        // Generate a cache key based on relevant fields
        var cacheKey = GenerateCacheKey(request);

        var existingCache = await _emailCacheRepository.GetByCacheKeyAsync(cacheKey);

        if (existingCache != null && !existingCache.IsDeleted && existingCache.ExpiresAt > DateTime.UtcNow)
        {
            // Update existing cache entry
            existingCache.MessageCount++;
            existingCache.UpdatedAt = DateTime.UtcNow;
            // Extend expiration if needed, or keep original
            existingCache.ExpiresAt = DateTime.UtcNow.AddMinutes(request.CacheExpirationMinutes);

            var updatedCache = await _emailCacheRepository.UpdateAsync(existingCache);

            // Create a new EmailMessage entry with status 'Cached' and link it to the cache
            var newEmailMessage = request.ToEmailMessageEntity();
            newEmailMessage.Status = EmailStatus.Cached;
            newEmailMessage.StatusMessage = $"Cached under key: {updatedCache.CacheKey}. Message count: {updatedCache.MessageCount}";
            var createdEmailMessage = await _emailMessageRepository.AddAsync(newEmailMessage);

            await _emailHistoryRepository.AddAsync(new EmailHistory
            {
                EmailMessageId = createdEmailMessage.Id,
                Action = "Cached",
                Details = $"Email added to existing cache entry '{updatedCache.CacheKey}'. Total messages: {updatedCache.MessageCount}",
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Email for {Subject} to {ToAddresses} added to existing cache {CacheKey}. Total count: {Count}",
                request.Subject, request.ToAddresses, cacheKey, updatedCache.MessageCount);

            return createdEmailMessage.ToResponse();
        }
        else
        {
            // Create a new cache entry
            var newCache = request.ToEmailCacheEntity(cacheKey);
            var createdCache = await _emailCacheRepository.AddAsync(newCache);

            // Create an EmailMessage entry with status 'Cached'
            var newEmailMessage = request.ToEmailMessageEntity();
            newEmailMessage.Status = EmailStatus.Cached;
            newEmailMessage.StatusMessage = $"New cache entry created: {createdCache.CacheKey}";
            var createdEmailMessage = await _emailMessageRepository.AddAsync(newEmailMessage);

            await _emailHistoryRepository.AddAsync(new EmailHistory
            {
                EmailMessageId = createdEmailMessage.Id,
                Action = "Cached",
                Details = $"New cache entry '{createdCache.CacheKey}' created. Message count: {createdCache.MessageCount}",
                CreatedAt = DateTime.UtcNow
            });

            _logger.LogInformation("Email for {Subject} to {ToAddresses} created new cache {CacheKey}.",
                request.Subject, request.ToAddresses, cacheKey);

            return createdEmailMessage.ToResponse();
        }
    }

    /// <inheritdoc />
    public async Task<int> ProcessExpiredCacheAsync()
    {
        var expiredEntries = await _emailCacheRepository.GetExpiredEntriesAsync();
        int processedCount = 0;

        foreach (var cacheEntry in expiredEntries)
        {
            _logger.LogInformation("Processing expired cache entry {CacheKey} with {MessageCount} messages.",
                cacheEntry.CacheKey, cacheEntry.MessageCount);

            // Construct a consolidated email from the cache entry
            var consolidatedEmailRequest = new SendEmailRequest
            {
                CompanyCode = cacheEntry.CompanyCode,
                ApplicationCode = cacheEntry.ApplicationCode,
                FromAddress = cacheEntry.FromAddress,
                ToAddresses = cacheEntry.ToAddresses,
                CcAddresses = cacheEntry.CcAddresses,
                BccAddresses = cacheEntry.BccAddresses,
                Subject = $"Consolidated: {cacheEntry.Subject} ({cacheEntry.MessageCount} messages)",
                Body = $"This is a consolidated email containing {cacheEntry.MessageCount} messages originally sent with subject: '{cacheEntry.Subject}'.\n\n" +
                       $"Original Body:\n{cacheEntry.Body}",
                Header = cacheEntry.Header,
                Footer = cacheEntry.Footer,
                ImportanceFlag = (EmailImportanceDto)cacheEntry.ImportanceFlag,
                MaxRetries = 3 // Default retries for consolidated email
            };

            try
            {
                // Send the consolidated email
                var sentEmailResponse = await _emailSenderService.SendEmailAsync(consolidatedEmailRequest);

                // Mark cache entry as deleted (or processed)
                cacheEntry.IsDeleted = true;
                cacheEntry.UpdatedAt = DateTime.UtcNow;
                await _emailCacheRepository.UpdateAsync(cacheEntry);

                // Log history for the cache entry processing
                await _emailHistoryRepository.AddAsync(new EmailHistory
                {
                    EmailMessageId = sentEmailResponse.Id, // Link to the newly sent consolidated email
                    Action = "Cache Processed",
                    Details = $"Cache entry '{cacheEntry.CacheKey}' processed and consolidated email sent (ID: {sentEmailResponse.Id}). Original message count: {cacheEntry.MessageCount}",
                    CreatedAt = DateTime.UtcNow
                });

                processedCount++;
                _logger.LogInformation("Cache entry {CacheKey} successfully processed and consolidated email sent (ID: {EmailId}).",
                    cacheEntry.CacheKey, sentEmailResponse.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process cache entry {CacheKey}. Error: {ErrorMessage}",
                    cacheEntry.CacheKey, ex.Message);
                // Optionally, update cache entry status to indicate processing failure or extend expiration
                // For now, we'll just log and move on.
            }
        }
        return processedCount;
    }

    /// <summary>
    /// Generates a unique cache key for an email based on its content and recipients.
    /// </summary>
    private static string GenerateCacheKey(SendCachedEmailRequest request)
    {
        // Normalize addresses and subject for consistent hashing
        var normalizedTo = string.Join(";", request.ToAddresses.Split(';', ',').Select(a => a.Trim().ToLowerInvariant()).OrderBy(a => a));
        var normalizedCc = string.Join(";", (request.CcAddresses ?? "").Split(';', ',').Select(a => a.Trim().ToLowerInvariant()).OrderBy(a => a));
        var normalizedBcc = string.Join(";", (request.BccAddresses ?? "").Split(';', ',').Select(a => a.Trim().ToLowerInvariant()).OrderBy(a => a));
        var normalizedSubject = request.Subject.Trim().ToLowerInvariant();
        var normalizedBodyHash = GetSha256Hash(request.Body); // Hash body to avoid very long keys

        var keyString = $"{request.CompanyCode}|{request.ApplicationCode}|{request.FromAddress.ToLowerInvariant()}|{normalizedTo}|{normalizedCc}|{normalizedBcc}|{normalizedSubject}|{normalizedBodyHash}|{(int)request.ImportanceFlag}";

        return GetSha256Hash(keyString);
    }

    /// <summary>
    /// Computes the SHA256 hash of a string.
    /// </summary>
    private static string GetSha256Hash(string input)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
