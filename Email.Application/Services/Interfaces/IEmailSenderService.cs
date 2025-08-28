using Email.Contracts.Requests;
using Email.Contracts.Responses;

namespace Email.Application.Services.Interfaces;

/// <summary>
/// Defines the contract for sending email messages.
/// </summary>
public interface IEmailSenderService
{
    /// <summary>
    /// Sends an email immediately.
    /// </summary>
    /// <param name="request">The request containing email details to send.</param>
    /// <returns>The created email message response with its initial status.</returns>
    Task<EmailResponse> SendEmailAsync(SendEmailRequest request);

    /// <summary>
    /// Processes and sends a batch of pending emails.
    /// This method is typically called by a background worker.
    /// </summary>
    /// <param name="companyCode">Optional company code to filter emails.</param>
    /// <param name="applicationCode">Optional application code to filter emails.</param>
    /// <returns>The number of emails successfully processed and sent.</returns>
    Task<int> ProcessPendingEmailsAsync(string? companyCode = null, string? applicationCode = null);

    /// <summary>
    /// Retries sending a specific failed email.
    /// </summary>
    /// <param name="emailId">The ID of the email to retry.</param>
    /// <returns>The updated email message response.</returns>
    Task<EmailResponse> RetryFailedEmailAsync(long emailId);
}
