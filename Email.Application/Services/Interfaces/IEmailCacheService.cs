using Email.Contracts.Requests;
using Email.Contracts.Responses;

namespace Email.Application.Services.Interfaces;

/// <summary>
/// Defines the contract for managing cached email messages for consolidation.
/// </summary>
public interface IEmailCacheService
{
    /// <summary>
    /// Sends an email, potentially caching it for consolidation if a similar email exists.
    /// </summary>
    /// <param name="request">The request containing email details to send/cache.</param>
    /// <returns>The created or updated email message response (could be a cached one).</returns>
    Task<EmailResponse> SendCachedEmailAsync(SendCachedEmailRequest request);

    /// <summary>
    /// Processes expired cached emails, consolidating and sending them.
    /// This method is typically called by a background worker.
    /// </summary>
    /// <returns>The number of cached emails successfully processed and sent.</returns>
    Task<int> ProcessExpiredCacheAsync();
}
