using Email.Infrastructure.Entities;

namespace Email.Infrastructure.Repositories.Interfaces;

/// <summary>
/// Repository interface for EmailMessage entity with email-specific operations.
/// </summary>
public interface IEmailMessageRepository : IBaseRepository<EmailMessage>
{
    /// <summary>
    /// Gets unsent messages for a company and application.
    /// </summary>
    /// <param name="companyCode">The company code.</param>
    /// <param name="applicationCode">The application code (supports wildcards).</param>
    /// <returns>A collection of unsent email messages.</returns>
    Task<IEnumerable<EmailMessage>> GetUnsentMessagesAsync(string companyCode, string? applicationCode = null);

    /// <summary>
    /// Gets messages by status.
    /// </summary>
    /// <param name="status">The email status.</param>
    /// <returns>A collection of email messages with the specified status.</returns>
    Task<IEnumerable<EmailMessage>> GetByStatusAsync(string status);

    /// <summary>
    /// Gets messages by company and application codes.
    /// </summary>
    /// <param name="companyCode">The company code.</param>
    /// <param name="applicationCode">The application code.</param>
    /// <returns>A collection of email messages.</returns>
    Task<IEnumerable<EmailMessage>> GetByCompanyAndApplicationAsync(string companyCode, string applicationCode);

    /// <summary>
    /// Updates the status of an email message.
    /// </summary>
    /// <param name="id">The email message ID.</param>
    /// <param name="status">The new status.</param>
    /// <param name="errorMessage">Optional error message.</param>
    /// <returns>True if updated successfully, false otherwise.</returns>
    Task<bool> UpdateStatusAsync(long id, string status, string? errorMessage = null);

    /// <summary>
    /// Increments the retry count for an email message.
    /// </summary>
    /// <param name="id">The email message ID.</param>
    /// <returns>True if updated successfully, false otherwise.</returns>
    Task<bool> IncrementRetryCountAsync(long id);

    /// <summary>
    /// Marks an email as sent.
    /// </summary>
    /// <param name="id">The email message ID.</param>
    /// <returns>True if updated successfully, false otherwise.</returns>
    Task<bool> MarkAsSentAsync(long id);

    /// <summary>
    /// Gets failed messages that can be retried.
    /// </summary>
    /// <returns>A collection of failed email messages that haven't exceeded max retries.</returns>
    Task<IEnumerable<EmailMessage>> GetFailedMessagesForRetryAsync();

    /// <summary>
    /// Gets email statistics by company and application.
    /// </summary>
    /// <param name="companyCode">The company code.</param>
    /// <param name="applicationCode">The application code.</param>
    /// <returns>Email statistics.</returns>
    Task<object> GetEmailStatisticsAsync(string companyCode, string? applicationCode = null);
}
