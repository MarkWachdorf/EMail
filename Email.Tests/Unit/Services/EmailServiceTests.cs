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

public class EmailServiceTests
{
    private readonly Mock<IEmailMessageRepository> _mockEmailMessageRepository;
    private readonly Mock<IEmailHistoryRepository> _mockEmailHistoryRepository;
    private readonly Mock<IErrorLogRepository> _mockErrorLogRepository;
    private readonly Mock<IEmailSender> _mockEmailSender;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockEmailMessageRepository = new Mock<IEmailMessageRepository>();
        _mockEmailHistoryRepository = new Mock<IEmailHistoryRepository>();
        _mockErrorLogRepository = new Mock<IErrorLogRepository>();
        _mockEmailSender = new Mock<IEmailSender>();
        _mockLogger = new Mock<ILogger<EmailService>>();
        _emailService = new EmailService(
            _mockEmailMessageRepository.Object, 
            _mockEmailHistoryRepository.Object,
            _mockErrorLogRepository.Object,
            _mockEmailSender.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task CreateEmailAsync_ValidRequest_ShouldCreateEmailAndLogHistory()
    {
        // Arrange
        var request = new CreateEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            ImportanceFlag = EmailImportanceDto.Normal,
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
            ImportanceFlag = EmailImportanceFlag.Normal,
            MaxRetries = request.MaxRetries,
            Status = EmailStatus.Pending,
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.AddAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(createdEmail);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        _mockEmailMessageRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(createdEmail);

        _mockErrorLogRepository.Setup(x => x.AddAsync(It.IsAny<ErrorLog>()))
            .ReturnsAsync(new ErrorLog());

        _mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(true);

        // Act
        var result = await _emailService.CreateEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.CompanyCode.Should().Be(request.CompanyCode);
        result.ApplicationCode.Should().Be(request.ApplicationCode);
        result.FromAddress.Should().Be(request.FromAddress);
        result.ToAddresses.Should().Be(request.ToAddresses);
        result.Subject.Should().Be(request.Subject);
        result.Body.Should().Be(request.Body);
        result.Status.Should().Be(EmailStatusDto.Pending);
        result.ImportanceFlag.Should().Be(EmailImportanceDto.Normal);
        result.RetryCount.Should().Be(0);
        result.MaxRetries.Should().Be(request.MaxRetries);

        _mockEmailMessageRepository.Verify(x => x.AddAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetEmailByIdAsync_ExistingEmail_ShouldReturnEmail()
    {
        // Arrange
        var emailId = 1L;
        var email = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Status = EmailStatus.Sent,
            ImportanceFlag = EmailImportanceFlag.Normal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync(email);

        // Act
        var result = await _emailService.GetEmailByIdAsync(emailId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(emailId);
        result.CompanyCode.Should().Be(email.CompanyCode);
        result.Status.Should().Be(EmailStatusDto.Sent);

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
    }

    [Fact]
    public async Task GetEmailByIdAsync_NonExistingEmail_ShouldReturnNull()
    {
        // Arrange
        var emailId = 999L;
        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync((EmailMessage?)null);

        // Act
        var result = await _emailService.GetEmailByIdAsync(emailId);

        // Assert
        result.Should().BeNull();
        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
    }

    [Fact]
    public async Task GetAllEmailsAsync_WithFilters_ShouldReturnFilteredResults()
    {
        // Arrange
        var emails = new List<EmailMessage>
        {
            new EmailMessage
            {
                Id = 1,
                CompanyCode = "TEST",
                ApplicationCode = "APP1",
                FromAddress = "test@example.com",
                ToAddresses = "user@example.com",
                Subject = "Test Subject 1",
                Body = "Test Body 1",
                Status = EmailStatus.Sent,
                ImportanceFlag = EmailImportanceFlag.Normal,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EmailMessage
            {
                Id = 2,
                CompanyCode = "TEST",
                ApplicationCode = "APP2",
                FromAddress = "test@example.com",
                ToAddresses = "user2@example.com",
                Subject = "Test Subject 2",
                Body = "Test Body 2",
                Status = EmailStatus.Pending,
                ImportanceFlag = EmailImportanceFlag.High,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _mockEmailMessageRepository.Setup(x => x.GetAllAsync())
            .ReturnsAsync(emails);

        // Act
        var result = await _emailService.GetAllEmailsAsync("TEST", "APP1", 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Succeeded.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCount(1);
        result.TotalRecords.Should().Be(1);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(10);

        var firstEmail = result.Data.First();
        firstEmail.CompanyCode.Should().Be("TEST");
        firstEmail.ApplicationCode.Should().Be("APP1");

        _mockEmailMessageRepository.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateEmailStatusAsync_ValidRequest_ShouldUpdateStatusAndLogHistory()
    {
        // Arrange
        var emailId = 1L;
        var rowVersion = new byte[] { 1, 2, 3, 4 };
        var request = new UpdateEmailStatusRequest
        {
            EmailId = emailId,
            NewStatus = EmailStatusDto.Sent,
            StatusMessage = "Email sent successfully",
            RowVersion = rowVersion
        };

        var existingEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Status = EmailStatus.Pending,
            ImportanceFlag = EmailImportanceFlag.Normal,
            RowVersion = rowVersion,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var updatedEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Status = EmailStatus.Sent,
            StatusMessage = request.StatusMessage,
            ImportanceFlag = EmailImportanceFlag.Normal,
            RowVersion = new byte[] { 5, 6, 7, 8 },
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync(existingEmail);

        _mockEmailMessageRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(updatedEmail);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailService.UpdateEmailStatusAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(emailId);
        result.Status.Should().Be(EmailStatusDto.Sent);
        result.StatusMessage.Should().Be(request.StatusMessage);

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.Once);
    }

    [Fact]
    public async Task UpdateEmailStatusAsync_EmailNotFound_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var emailId = 999L;
        var request = new UpdateEmailStatusRequest
        {
            EmailId = emailId,
            NewStatus = EmailStatusDto.Sent,
            StatusMessage = "Email sent successfully",
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync((EmailMessage?)null);

        // Act & Assert
        var action = () => _emailService.UpdateEmailStatusAsync(request);
        await action.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Email with ID {emailId} not found or is deleted.");

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmailStatusAsync_ConcurrencyConflict_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var emailId = 1L;
        var request = new UpdateEmailStatusRequest
        {
            EmailId = emailId,
            NewStatus = EmailStatusDto.Sent,
            StatusMessage = "Email sent successfully",
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };

        var existingEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Status = EmailStatus.Pending,
            ImportanceFlag = EmailImportanceFlag.Normal,
            RowVersion = new byte[] { 5, 6, 7, 8 }, // Different row version
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync(existingEmail);

        // Act & Assert
        var action = () => _emailService.UpdateEmailStatusAsync(request);
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Concurrency conflict: Email has been modified by another process.");

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task SoftDeleteEmailAsync_ValidRequest_ShouldSoftDeleteAndLogHistory()
    {
        // Arrange
        var emailId = 1L;
        var rowVersion = new byte[] { 1, 2, 3, 4 };

        var existingEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Status = EmailStatus.Sent,
            ImportanceFlag = EmailImportanceFlag.Normal,
            RowVersion = rowVersion,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var deletedEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Status = EmailStatus.Sent,
            ImportanceFlag = EmailImportanceFlag.Normal,
            RowVersion = new byte[] { 5, 6, 7, 8 },
            IsDeleted = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync(existingEmail);

        _mockEmailMessageRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(deletedEmail);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailService.SoftDeleteEmailAsync(emailId, rowVersion);

        // Assert
        result.Should().BeTrue();

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.Once);
    }

    [Fact]
    public async Task SoftDeleteEmailAsync_EmailNotFound_ShouldReturnFalse()
    {
        // Arrange
        var emailId = 999L;
        var rowVersion = new byte[] { 1, 2, 3, 4 };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync((EmailMessage?)null);

        // Act
        var result = await _emailService.SoftDeleteEmailAsync(emailId, rowVersion);

        // Assert
        result.Should().BeFalse();

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Fact]
    public async Task SoftDeleteEmailAsync_AlreadyDeleted_ShouldReturnFalse()
    {
        // Arrange
        var emailId = 1L;
        var rowVersion = new byte[] { 1, 2, 3, 4 };

        var existingEmail = new EmailMessage
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "user@example.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Status = EmailStatus.Sent,
            ImportanceFlag = EmailImportanceFlag.Normal,
            RowVersion = rowVersion,
            IsDeleted = true, // Already deleted
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailMessageRepository.Setup(x => x.GetByIdAsync(emailId))
            .ReturnsAsync(existingEmail);

        // Act
        var result = await _emailService.SoftDeleteEmailAsync(emailId, rowVersion);

        // Assert
        result.Should().BeFalse();

        _mockEmailMessageRepository.Verify(x => x.GetByIdAsync(emailId), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailMessage>()), Times.Never);
    }
}
