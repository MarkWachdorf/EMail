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
/// Service for managing email messages, including creation, retrieval, and status updates.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IEmailMessageRepository _emailMessageRepository;
    private readonly IEmailHistoryRepository _emailHistoryRepository;
    private readonly IErrorLogRepository _errorLogRepository;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IEmailMessageRepository emailMessageRepository, 
        IEmailHistoryRepository emailHistoryRepository,
        IErrorLogRepository errorLogRepository,
        IEmailSender emailSender,
        ILogger<EmailService> logger)
    {
        _emailMessageRepository = emailMessageRepository ?? throw new ArgumentNullException(nameof(emailMessageRepository));
        _emailHistoryRepository = emailHistoryRepository ?? throw new ArgumentNullException(nameof(emailHistoryRepository));
        _errorLogRepository = errorLogRepository ?? throw new ArgumentNullException(nameof(errorLogRepository));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<EmailResponse> CreateEmailAsync(CreateEmailRequest request)
    {
        var emailMessage = request.ToEntity();
        emailMessage.Status = EmailStatus.Pending; // Ensure status is pending on creation

        var createdEmail = await _emailMessageRepository.AddAsync(emailMessage);

        // Log history
        await _emailHistoryRepository.AddAsync(new EmailHistory
        {
            EmailMessageId = createdEmail.Id,
            Action = "Created",
            Details = $"Email created with initial status: {createdEmail.Status}",
            CreatedAt = DateTime.UtcNow
        });

        // Attempt to send the email immediately after creation
        await AttemptSendEmailAsync(createdEmail);

        return createdEmail.ToResponse();
    }

    /// <inheritdoc />
    public async Task<EmailResponse?> GetEmailByIdAsync(long id)
    {
        var emailMessage = await _emailMessageRepository.GetByIdAsync(id);
        return emailMessage?.ToResponse();
    }

    /// <inheritdoc />
    public async Task<PagedResponse<IEnumerable<EmailResponse>>> GetAllEmailsAsync(string? companyCode, string? applicationCode, int pageNumber, int pageSize)
    {
        // For simplicity, this example fetches all and then filters/pages in memory.
        // In a real-world scenario, this would be pushed to the repository for efficient database querying.
        var allEmails = await _emailMessageRepository.GetAllAsync();

        var filteredEmails = allEmails
            .Where(e => !e.IsDeleted &&
                        (string.IsNullOrEmpty(companyCode) || e.CompanyCode.Equals(companyCode, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(applicationCode) || e.ApplicationCode.Equals(applicationCode, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var totalRecords = filteredEmails.Count;
        var pagedEmails = filteredEmails
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResponse<IEnumerable<EmailResponse>>.Success(
            pagedEmails.Select(e => e.ToResponse()),
            pageNumber,
            pageSize,
            totalRecords,
            "Emails retrieved successfully."
        );
    }

    /// <inheritdoc />
    public async Task<EmailResponse> UpdateEmailStatusAsync(UpdateEmailStatusRequest request)
    {
        var emailMessage = await _emailMessageRepository.GetByIdAsync(request.EmailId);

        if (emailMessage == null || emailMessage.IsDeleted)
        {
            throw new KeyNotFoundException($"Email with ID {request.EmailId} not found or is deleted.");
        }

        // Optimistic concurrency check
        if (!emailMessage.RowVersion.SequenceEqual(request.RowVersion))
        {
            throw new InvalidOperationException("Concurrency conflict: Email has been modified by another process.");
        }

        var oldStatus = emailMessage.Status;
        emailMessage.Status = (EmailStatus)request.NewStatus;
        emailMessage.StatusMessage = request.StatusMessage;
        emailMessage.UpdatedAt = DateTime.UtcNow;

        var updatedEmail = await _emailMessageRepository.UpdateAsync(emailMessage);

        // Log history
        await _emailHistoryRepository.AddAsync(new EmailHistory
        {
            EmailMessageId = updatedEmail.Id,
            Action = "Status Updated",
            Details = $"Status changed from {oldStatus} to {updatedEmail.Status}. Message: {updatedEmail.StatusMessage}",
            CreatedAt = DateTime.UtcNow
        });

        return updatedEmail.ToResponse();
    }

    /// <inheritdoc />
    public async Task<bool> SoftDeleteEmailAsync(long id, byte[] rowVersion)
    {
        var emailMessage = await _emailMessageRepository.GetByIdAsync(id);

        if (emailMessage == null || emailMessage.IsDeleted)
        {
            return false; // Already deleted or not found
        }

        // Optimistic concurrency check
        if (!emailMessage.RowVersion.SequenceEqual(rowVersion))
        {
            throw new InvalidOperationException("Concurrency conflict: Email has been modified by another process.");
        }

        emailMessage.IsDeleted = true;
        emailMessage.UpdatedAt = DateTime.UtcNow;

        var deletedEmail = await _emailMessageRepository.UpdateAsync(emailMessage);

        // Log history
        await _emailHistoryRepository.AddAsync(new EmailHistory
        {
            EmailMessageId = deletedEmail.Id,
            Action = "Soft Deleted",
            Details = "Email marked as soft deleted.",
            CreatedAt = DateTime.UtcNow
        });

        return deletedEmail.IsDeleted;
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
            // Send the email using the actual email sender
            _logger.LogInformation("Attempting to send email {EmailId} to {ToAddresses} (Attempt {RetryCount})",
                emailMessage.Id, emailMessage.ToAddresses, emailMessage.RetryCount);

            // Send the email using the actual email sender
            bool sendSuccessful = await _emailSender.SendEmailAsync(emailMessage);

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
                    Source = nameof(EmailService),
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
                Source = nameof(EmailService),
                Message = errorMessage,
                StackTrace = ex.StackTrace,
                CompanyCode = emailMessage.CompanyCode,
                ApplicationCode = emailMessage.ApplicationCode,
                AdditionalData = $"Subject: {emailMessage.Subject}, To: {emailMessage.ToAddresses}"
            });
            _logger.LogError(ex, "Exception occurred while sending email {EmailId}.", emailMessage.Id);
        }
    }


}
