using Email.Application.Mappers;
using Email.Application.Services.Interfaces;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Email.Infrastructure.Entities;
using Email.Infrastructure.Entities.Enums;
using Email.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Email.Application.Services;

/// <summary>
/// Service responsible for the actual sending of emails and managing retry logic.
/// </summary>
public class EmailSenderService : IEmailSenderService
{
    private readonly IEmailMessageRepository _emailMessageRepository;
    private readonly IEmailHistoryRepository _emailHistoryRepository;
    private readonly IErrorLogRepository _errorLogRepository;
    private readonly ILogger<EmailSenderService> _logger;

    public EmailSenderService(
        IEmailMessageRepository emailMessageRepository,
        IEmailHistoryRepository emailHistoryRepository,
        IErrorLogRepository errorLogRepository,
        ILogger<EmailSenderService> logger)
    {
        _emailMessageRepository = emailMessageRepository ?? throw new ArgumentNullException(nameof(emailMessageRepository));
        _emailHistoryRepository = emailHistoryRepository ?? throw new ArgumentNullException(nameof(emailHistoryRepository));
        _errorLogRepository = errorLogRepository ?? throw new ArgumentNullException(nameof(errorLogRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<EmailResponse> SendEmailAsync(SendEmailRequest request)
    {
        var emailMessage = request.ToEntity();
        emailMessage.Status = EmailStatus.Pending; // Always start as pending

        var createdEmail = await _emailMessageRepository.AddAsync(emailMessage);

        // Log creation history
        await _emailHistoryRepository.AddAsync(new EmailHistory
        {
            EmailMessageId = createdEmail.Id,
            Action = "Created",
            Details = $"Email created for immediate sending. Initial status: {createdEmail.Status}",
            CreatedAt = DateTime.UtcNow
        });

        // Attempt to send immediately (can be moved to a background worker for true async)
        await AttemptSendEmailAsync(createdEmail);

        return createdEmail.ToResponse();
    }

    /// <inheritdoc />
    public async Task<int> ProcessPendingEmailsAsync(string? companyCode = null, string? applicationCode = null)
    {
        var unsentMessages = await _emailMessageRepository.GetUnsentMessagesAsync(companyCode ?? string.Empty, applicationCode);
        int sentCount = 0;

        foreach (var message in unsentMessages)
        {
            if (message.Status == EmailStatus.Pending && message.RetryCount <= message.MaxRetries)
            {
                await AttemptSendEmailAsync(message);
                sentCount++;
            }
            else if (message.Status == EmailStatus.Pending && message.RetryCount > message.MaxRetries)
            {
                // Mark as failed if retries exceeded
                message.Status = EmailStatus.Failed;
                message.StatusMessage = $"Failed: Max retries ({message.MaxRetries}) exceeded.";
                message.UpdatedAt = DateTime.UtcNow;
                await _emailMessageRepository.UpdateAsync(message);

                await _emailHistoryRepository.AddAsync(new EmailHistory
                {
                    EmailMessageId = message.Id,
                    Action = "Failed",
                    Details = message.StatusMessage,
                    CreatedAt = DateTime.UtcNow
                });

                await _errorLogRepository.AddAsync(new ErrorLog
                {
                    Level = Email.Infrastructure.Entities.Enums.LogLevel.Error,
                    Source = nameof(EmailSenderService),
                    Message = $"Email {message.Id} failed after max retries.",
                    CompanyCode = message.CompanyCode,
                    ApplicationCode = message.ApplicationCode,
                    AdditionalData = $"Subject: {message.Subject}, To: {message.ToAddresses}"
                });
            }
        }
        return sentCount;
    }

    /// <inheritdoc />
    public async Task<EmailResponse> RetryFailedEmailAsync(long emailId)
    {
        var emailMessage = await _emailMessageRepository.GetByIdAsync(emailId);

        if (emailMessage == null || emailMessage.IsDeleted)
        {
            throw new KeyNotFoundException($"Email with ID {emailId} not found or is deleted.");
        }

        if (emailMessage.Status != EmailStatus.Failed)
        {
            throw new InvalidOperationException($"Email with ID {emailId} is not in 'Failed' status and cannot be retried.");
        }

        // Reset status to pending (retry count will be incremented in AttemptSendEmailAsync)
        emailMessage.Status = EmailStatus.Pending;
        emailMessage.StatusMessage = "Retrying...";
        emailMessage.UpdatedAt = DateTime.UtcNow;

        var updatedEmail = await _emailMessageRepository.UpdateAsync(emailMessage);

        await _emailHistoryRepository.AddAsync(new EmailHistory
        {
            EmailMessageId = updatedEmail.Id,
            Action = "Retried",
            Details = $"Email retry attempt {updatedEmail.RetryCount}.",
            CreatedAt = DateTime.UtcNow
        });

        // Attempt to send immediately
        await AttemptSendEmailAsync(updatedEmail);

        return updatedEmail.ToResponse();
    }

    /// <summary>
    /// Internal method to attempt sending a single email.
    /// </summary>
    /// <param name="emailMessage">The email message entity to send.</param>
    private async Task AttemptSendEmailAsync(EmailMessage emailMessage)
    {
        emailMessage.LastAttemptedAt = DateTime.UtcNow;
        emailMessage.RetryCount++;
        emailMessage.UpdatedAt = DateTime.UtcNow;

        try
        {
            // Simulate actual email sending logic
            // In a real application, this would integrate with an external email service (e.g., SendGrid, Mailgun, SMTP client)
            _logger.LogInformation("Attempting to send email {EmailId} to {ToAddresses} (Attempt {RetryCount})",
                emailMessage.Id, emailMessage.ToAddresses, emailMessage.RetryCount);

            // Simulate success or failure
            bool sendSuccessful = SimulateEmailSend(emailMessage); // Replace with actual sending logic

            if (sendSuccessful)
            {
                emailMessage.Status = EmailStatus.Sent;
                emailMessage.StatusMessage = "Successfully sent.";
                await _emailMessageRepository.UpdateAsync(emailMessage);

                await _emailHistoryRepository.AddAsync(new EmailHistory
                {
                    EmailMessageId = emailMessage.Id,
                    Action = "Sent",
                    Details = "Email successfully sent.",
                    CreatedAt = DateTime.UtcNow
                });
                _logger.LogInformation("Email {EmailId} sent successfully.", emailMessage.Id);
            }
            else
            {
                string failureMessage = $"Failed to send (attempt {emailMessage.RetryCount}).";
                if (emailMessage.RetryCount > emailMessage.MaxRetries)
                {
                    emailMessage.Status = EmailStatus.Failed;
                    failureMessage += $" Max retries ({emailMessage.MaxRetries}) exceeded.";
                }
                else
                {
                    emailMessage.Status = EmailStatus.Pending; // Keep pending for retry
                }
                emailMessage.StatusMessage = failureMessage;
                await _emailMessageRepository.UpdateAsync(emailMessage);

                await _emailHistoryRepository.AddAsync(new EmailHistory
                {
                    EmailMessageId = emailMessage.Id,
                    Action = "Failed Attempt",
                    Details = failureMessage,
                    CreatedAt = DateTime.UtcNow
                });

                await _errorLogRepository.AddAsync(new ErrorLog
                {
                    Level = Email.Infrastructure.Entities.Enums.LogLevel.Warning,
                    Source = nameof(EmailSenderService),
                    Message = $"Email {emailMessage.Id} failed to send (attempt {emailMessage.RetryCount}).",
                    CompanyCode = emailMessage.CompanyCode,
                    ApplicationCode = emailMessage.ApplicationCode,
                    AdditionalData = $"Subject: {emailMessage.Subject}, To: {emailMessage.ToAddresses}, Status: {emailMessage.Status}"
                });
                _logger.LogWarning("Email {EmailId} failed to send (attempt {RetryCount}). Status: {Status}",
                    emailMessage.Id, emailMessage.RetryCount, emailMessage.Status);
            }
        }
        catch (Exception ex)
        {
            string errorMessage = $"Exception while sending email {emailMessage.Id}: {ex.Message}";
            emailMessage.Status = EmailStatus.Failed; // Mark as failed on unexpected exception
            emailMessage.StatusMessage = errorMessage;
            await _emailMessageRepository.UpdateAsync(emailMessage);

            await _emailHistoryRepository.AddAsync(new EmailHistory
            {
                EmailMessageId = emailMessage.Id,
                Action = "Failed",
                Details = errorMessage,
                CreatedAt = DateTime.UtcNow
            });

                            await _errorLogRepository.AddAsync(new ErrorLog
                {
                    Level = Email.Infrastructure.Entities.Enums.LogLevel.Error,
                    Source = nameof(EmailSenderService),
                Message = errorMessage,
                StackTrace = ex.StackTrace,
                CompanyCode = emailMessage.CompanyCode,
                ApplicationCode = emailMessage.ApplicationCode,
                AdditionalData = $"Subject: {emailMessage.Subject}, To: {emailMessage.ToAddresses}"
            });
            _logger.LogError(ex, "Exception occurred while sending email {EmailId}.", emailMessage.Id);
        }
    }

    /// <summary>
    /// Simulates an email sending operation. Replace with actual integration.
    /// </summary>
    /// <param name="emailMessage">The email message to simulate sending.</param>
    /// <returns>True if the simulation is successful, false otherwise.</returns>
    private bool SimulateEmailSend(EmailMessage emailMessage)
    {
        // For testing: always succeed when sending to the test email address
        if (emailMessage.ToAddresses?.Contains("wachdorfm@hotmail.com") == true)
        {
            return true;
        }
        
        // Simulate a 70% success rate for demonstration purposes
        // Or simulate failure if retry count is 2 (to test max retries)
        if (emailMessage.RetryCount == 2) return false; // Force failure on 2nd attempt for testing retry logic
        return new Random().Next(100) < 70;
    }
}
