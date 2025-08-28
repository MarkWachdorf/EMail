using Email.Application.Services;
using Email.Application.Services.Interfaces;
using Email.Contracts.Enums;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Email.Infrastructure.Entities;
using Email.Infrastructure.Entities.Enums;
using Email.Infrastructure.Repositories.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Email.Tests.Unit.Services;

public class EmailSenderServiceTests
{
    private readonly Mock<IEmailMessageRepository> _mockEmailMessageRepository;
    private readonly Mock<IEmailHistoryRepository> _mockEmailHistoryRepository;
    private readonly Mock<IErrorLogRepository> _mockErrorLogRepository;
    private readonly Mock<ILogger<EmailSenderService>> _mockLogger;
    private readonly EmailSenderService _emailSenderService;

    public EmailSenderServiceTests()
    {
        _mockEmailMessageRepository = new Mock<IEmailMessageRepository>();
        _mockEmailHistoryRepository = new Mock<IEmailHistoryRepository>();
        _mockErrorLogRepository = new Mock<IErrorLogRepository>();
        _mockLogger = new Mock<ILogger<EmailSenderService>>();
        _emailSenderService = new EmailSenderService(
            _mockEmailMessageRepository.Object,
            _mockEmailHistoryRepository.Object,
            _mockErrorLogRepository.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendEmailAsync_ValidRequest_ShouldCreateEmailAndAttemptSending()
    {
        // Arrange
        var request = new SendEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Email",
            Body = "This is a test email body",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3
        };

        var createdEmail = new EmailMessage
        {
            Id = 1,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Importance = EmailImportance.Normal,
            MaxRetries = request.MaxRetries,
            Status = EmailStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var sentEmail = new EmailMessage
        {
            Id = 1,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Importance = EmailImportance.Normal,
            MaxRetries = request.MaxRetries,
            Status = EmailStatus.Sent,
            StatusMessage = "Successfully sent.",
            RetryCount = 1,
            LastAttemptedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.AddAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(createdEmail);

        _mockEmailMessageRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(sentEmail);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailSenderService.SendEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Status.Should().Be(EmailStatusDto.Sent);
        result.StatusMessage.Should().Be("Successfully sent.");

        _mockEmailMessageRepository.Verify(x => x.AddAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task ProcessPendingEmailsAsync_WithPendingEmails_ShouldProcessAndSend()
    {
        // Arrange
        var pendingEmails = new List<EmailMessage>
        {
            new EmailMessage
            {
                Id = 1,
                CompanyCode = "TEST",
                ApplicationCode = "APP1",
                FromAddress = "test@example.com",
                ToAddresses = "wachdorfm@hotmail.com",
                Subject = "Pending Email 1",
                Body = "Body 1",
                Status = EmailStatus.Pending,
                Importance = EmailImportance.Normal,
                RetryCount = 0,
                MaxRetries = 3,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EmailMessage
            {
                Id = 2,
                CompanyCode = "TEST",
                ApplicationCode = "APP1",
                FromAddress = "test@example.com",
                ToAddresses = "wachdorfm@hotmail.com",
                Subject = "Pending Email 2",
                Body = "Body 2",
                Status = EmailStatus.Pending,
                Importance = EmailImportance.High,
                RetryCount = 1,
                MaxRetries = 3,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var sentEmail = new EmailMessage
        {
            Id = 1,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Pending Email 1",
            Body = "Body 1",
            Status = EmailStatus.Sent,
            StatusMessage = "Successfully sent.",
            Importance = EmailImportance.Normal,
            RetryCount = 1,
            MaxRetries = 3,
            LastAttemptedAt = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetUnsentMessagesAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pendingEmails);

        _mockEmailMessageRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(sentEmail);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailSenderService.ProcessPendingEmailsAsync("TEST", "APP1");

        // Assert
        result.Should().Be(2); // Both emails should be processed

        _mockEmailMessageRepository.Verify(x => x.GetUnsentMessagesAsync("TEST", "APP1"), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Exactly(2));
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.AtLeast(2));
    }

    [Fact]
    public async Task ProcessPendingEmailsAsync_WithExceededRetries_ShouldMarkAsFailed()
    {
        // Arrange
        var failedEmails = new List<EmailMessage>
        {
            new EmailMessage
            {
                Id = 1,
                CompanyCode = "TEST",
                ApplicationCode = "APP1",
                FromAddress = "test@example.com",
                ToAddresses = "wachdorfm@hotmail.com",
                Subject = "Failed Email",
                Body = "Body",
                Status = EmailStatus.Pending,
                Importance = EmailImportance.Normal,
                RetryCount = 4, // Exceeds MaxRetries of 3
                MaxRetries = 3,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var updatedFailedEmail = new EmailMessage
        {
            Id = 1,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Failed Email",
            Body = "Body",
            Status = EmailStatus.Failed,
            StatusMessage = "Failed: Max retries (3) exceeded.",
            Importance = EmailImportance.Normal,
            RetryCount = 4,
            MaxRetries = 3,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetUnsentMessagesAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(failedEmails);

        _mockEmailMessageRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(updatedFailedEmail);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        _mockErrorLogRepository.Setup(x => x.AddAsync(It.IsAny<ErrorLog>()))
            .ReturnsAsync(new ErrorLog());

        // Act
        var result = await _emailSenderService.ProcessPendingEmailsAsync();

        // Assert
        result.Should().Be(0); // No emails sent, but one marked as failed

        _mockEmailMessageRepository.Verify(x => x.GetUnsentMessagesAsync(string.Empty, null), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.Once);
        _mockErrorLogRepository.Verify(x => x.AddAsync(It.IsAny<ErrorLog>()), Times.Once);
    }

    [Fact]
    public async Task RetryFailedEmailAsync_ValidFailedEmail_ShouldRetryAndSend()
    {
        // Arrange
        var emailId = 1L;
        var failedEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Failed Email",
            Body = "Body",
            Status = EmailStatus.Failed,
            Importance = EmailImportance.Normal,
            RetryCount = 2,
            MaxRetries = 3,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var retriedEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Failed Email",
            Body = "Body",
            Status = EmailStatus.Sent,
            StatusMessage = "Successfully sent.",
            Importance = EmailImportance.Normal,
            RetryCount = 3,
            MaxRetries = 3,
            LastAttemptedAt = DateTime.UtcNow,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync(failedEmail);

        _mockEmailMessageRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync((EmailMessage email) => email); // Return the updated email as-is

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailSenderService.RetryFailedEmailAsync(emailId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(emailId);
        result.Status.Should().Be(EmailStatusDto.Sent);
        result.RetryCount.Should().Be(3);

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.AtLeast(2)); // Called once for retry setup, once for sending
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task RetryFailedEmailAsync_EmailNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var emailId = 999L;

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync((EmailMessage?)null);

        // Act & Assert
        var action = () => _emailSenderService.RetryFailedEmailAsync(emailId);
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Email with ID {emailId} not found or is deleted.");

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task RetryFailedEmailAsync_EmailNotFailed_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var emailId = 1L;
        var sentEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Sent Email",
            Body = "Body",
            Status = EmailStatus.Sent, // Not failed
            Importance = EmailImportance.Normal,
            RetryCount = 1,
            MaxRetries = 3,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync(sentEmail);

        // Act & Assert
        var action = () => _emailSenderService.RetryFailedEmailAsync(emailId);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Email with ID {emailId} is not in 'Failed' status and cannot be retried.");

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task RetryFailedEmailAsync_EmailDeleted_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var emailId = 1L;
        var deletedEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Deleted Email",
            Body = "Body",
            Status = EmailStatus.Failed,
            Importance = EmailImportance.Normal,
            RetryCount = 1,
            MaxRetries = 3,
            IsDeleted = true, // Deleted
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync(deletedEmail);

        // Act & Assert
        var action = () => _emailSenderService.RetryFailedEmailAsync(emailId);
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Email with ID {emailId} not found or is deleted.");

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Never);
    }
}
