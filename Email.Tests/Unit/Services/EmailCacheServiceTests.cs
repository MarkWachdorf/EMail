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

public class EmailCacheServiceTests
{
    private readonly Mock<IEmailCacheRepository> _mockEmailCacheRepository;
    private readonly Mock<IEmailMessageRepository> _mockEmailMessageRepository;
    private readonly Mock<IEmailHistoryRepository> _mockEmailHistoryRepository;
    private readonly Mock<IEmailSenderService> _mockEmailSenderService;
    private readonly Mock<ILogger<EmailCacheService>> _mockLogger;
    private readonly EmailCacheService _emailCacheService;

    public EmailCacheServiceTests()
    {
        _mockEmailCacheRepository = new Mock<IEmailCacheRepository>();
        _mockEmailMessageRepository = new Mock<IEmailMessageRepository>();
        _mockEmailHistoryRepository = new Mock<IEmailHistoryRepository>();
        _mockEmailSenderService = new Mock<IEmailSenderService>();
        _mockLogger = new Mock<ILogger<EmailCacheService>>();
        _emailCacheService = new EmailCacheService(
            _mockEmailCacheRepository.Object,
            _mockEmailMessageRepository.Object,
            _mockEmailHistoryRepository.Object,
            _mockEmailSenderService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SendCachedEmailAsync_NewCacheEntry_ShouldCreateNewCacheAndEmail()
    {
        // Arrange
        var request = new SendCachedEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3,
            CacheExpirationMinutes = 60
        };

        var cacheKey = "test-cache-key";
        var newCache = new EmailCache
        {
            Id = 1,
            CacheKey = cacheKey,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Importance = EmailImportance.Normal,
            MessageCount = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.CacheExpirationMinutes),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var newEmailMessage = new EmailMessage
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
            Status = EmailStatus.Cached,
            StatusMessage = $"New cache entry created: {cacheKey}",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailCacheRepository.Setup(x => x.GetByCacheKeyAsync(It.IsAny<string>()))
            .ReturnsAsync((EmailCache?)null);

        _mockEmailCacheRepository.Setup(x => x.AddAsync(It.IsAny<EmailCache>()))
            .ReturnsAsync(newCache);

        _mockEmailMessageRepository.Setup(x => x.AddAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(newEmailMessage);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailCacheService.SendCachedEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Status.Should().Be(EmailStatusDto.Cached);
        result.StatusMessage.Should().Contain("New cache entry created:");

        _mockEmailCacheRepository.Verify(x => x.GetByCacheKeyAsync(It.IsAny<string>()), Times.Once);
        _mockEmailCacheRepository.Verify(x => x.AddAsync(It.IsAny<EmailCache>()), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.AddAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.Once);
    }

    [Fact]
    public async Task SendCachedEmailAsync_ExistingCacheEntry_ShouldUpdateCacheAndCreateEmail()
    {
        // Arrange
        var request = new SendCachedEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3,
            CacheExpirationMinutes = 60
        };

        var cacheKey = "test-cache-key";
        var existingCache = new EmailCache
        {
            Id = 1,
            CacheKey = cacheKey,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Importance = EmailImportance.Normal,
            MessageCount = 2,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-30)
        };

        var updatedCache = new EmailCache
        {
            Id = 1,
            CacheKey = cacheKey,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Importance = EmailImportance.Normal,
            MessageCount = 3, // Incremented
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.CacheExpirationMinutes),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            UpdatedAt = DateTime.UtcNow
        };

        var newEmailMessage = new EmailMessage
        {
            Id = 2,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Importance = EmailImportance.Normal,
            MaxRetries = request.MaxRetries,
            Status = EmailStatus.Cached,
            StatusMessage = $"Cached under key: {updatedCache.CacheKey}. Message count: {updatedCache.MessageCount}",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailCacheRepository.Setup(x => x.GetByCacheKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(existingCache);

        _mockEmailCacheRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailCache>()))
            .ReturnsAsync(updatedCache);

        _mockEmailMessageRepository.Setup(x => x.AddAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(newEmailMessage);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailCacheService.SendCachedEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(2);
        result.Status.Should().Be(EmailStatusDto.Cached);
        result.StatusMessage.Should().Contain("Cached under key:");
        result.StatusMessage.Should().Contain("Message count: 3");

        _mockEmailCacheRepository.Verify(x => x.GetByCacheKeyAsync(It.IsAny<string>()), Times.Once);
        _mockEmailCacheRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailCache>()), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.AddAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.Once);
    }

    [Fact]
    public async Task SendCachedEmailAsync_ExpiredCacheEntry_ShouldCreateNewCache()
    {
        // Arrange
        var request = new SendCachedEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3,
            CacheExpirationMinutes = 60
        };

        var cacheKey = "test-cache-key";
        var expiredCache = new EmailCache
        {
            Id = 1,
            CacheKey = cacheKey,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Importance = EmailImportance.Normal,
            MessageCount = 2,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10), // Expired
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-70),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-70)
        };

        var newCache = new EmailCache
        {
            Id = 2,
            CacheKey = cacheKey,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Importance = EmailImportance.Normal,
            MessageCount = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(request.CacheExpirationMinutes),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var newEmailMessage = new EmailMessage
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
            Status = EmailStatus.Cached,
            StatusMessage = $"New cache entry created: {cacheKey}",
            RetryCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailCacheRepository.Setup(x => x.GetByCacheKeyAsync(It.IsAny<string>()))
            .ReturnsAsync(expiredCache);

        _mockEmailCacheRepository.Setup(x => x.AddAsync(It.IsAny<EmailCache>()))
            .ReturnsAsync(newCache);

        _mockEmailMessageRepository.Setup(x => x.AddAsync(It.IsAny<EmailMessage>()))
            .ReturnsAsync(newEmailMessage);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailCacheService.SendCachedEmailAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Status.Should().Be(EmailStatusDto.Cached);
        result.StatusMessage.Should().Contain("New cache entry created:");

        _mockEmailCacheRepository.Verify(x => x.GetByCacheKeyAsync(It.IsAny<string>()), Times.Once);
        _mockEmailCacheRepository.Verify(x => x.AddAsync(It.IsAny<EmailCache>()), Times.Once);
        _mockEmailMessageRepository.Verify(x => x.AddAsync(It.IsAny<EmailMessage>()), Times.Once);
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.Once);
    }

    [Fact]
    public async Task ProcessExpiredCacheAsync_WithExpiredEntries_ShouldProcessAndSendConsolidated()
    {
        // Arrange
        var expiredEntries = new List<EmailCache>
        {
            new EmailCache
            {
                Id = 1,
                CacheKey = "cache-key-1",
                CompanyCode = "TEST",
                ApplicationCode = "APP1",
                FromAddress = "test@example.com",
                ToAddresses = "wachdorfm@hotmail.com",
                Subject = "Test Subject",
                Body = "Test Body",
                Importance = EmailImportance.Normal,
                MessageCount = 3,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-70),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-70)
            },
            new EmailCache
            {
                Id = 2,
                CacheKey = "cache-key-2",
                CompanyCode = "TEST",
                ApplicationCode = "APP2",
                FromAddress = "test@example.com",
                ToAddresses = "user2@example.com",
                Subject = "Another Subject",
                Body = "Another Body",
                Importance = EmailImportance.High,
                MessageCount = 2,
                ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-65),
                UpdatedAt = DateTime.UtcNow.AddMinutes(-65)
            }
        };

        var processedCache = new EmailCache
        {
            Id = 1,
            CacheKey = "cache-key-1",
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Importance = EmailImportance.Normal,
            MessageCount = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
            IsDeleted = true, // Marked as processed
            CreatedAt = DateTime.UtcNow.AddMinutes(-70),
            UpdatedAt = DateTime.UtcNow
        };

        var consolidatedEmailResponse = new EmailResponse
        {
            Id = 100,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Consolidated: Test Subject (3 messages)",
            Body = "This is a consolidated email containing 3 messages originally sent with subject: 'Test Subject'.\n\nOriginal Body:\nTest Body",
            Status = EmailStatusDto.Sent,
            Importance = EmailImportanceDto.Normal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailCacheRepository.Setup(x => x.GetExpiredEntriesAsync())
            .ReturnsAsync(expiredEntries);

        _mockEmailCacheRepository.Setup(x => x.UpdateAsync(It.IsAny<EmailCache>()))
            .ReturnsAsync(processedCache);

        _mockEmailSenderService.Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>()))
            .ReturnsAsync(consolidatedEmailResponse);

        _mockEmailHistoryRepository.Setup(x => x.AddAsync(It.IsAny<EmailHistory>()))
            .ReturnsAsync(new EmailHistory());

        // Act
        var result = await _emailCacheService.ProcessExpiredCacheAsync();

        // Assert
        result.Should().Be(2); // Both expired entries should be processed

        _mockEmailCacheRepository.Verify(x => x.GetExpiredEntriesAsync(), Times.Once);
        _mockEmailCacheRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailCache>()), Times.Exactly(2));
        _mockEmailSenderService.Verify(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>()), Times.Exactly(2));
        _mockEmailHistoryRepository.Verify(x => x.AddAsync(It.IsAny<EmailHistory>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessExpiredCacheAsync_NoExpiredEntries_ShouldReturnZero()
    {
        // Arrange
        _mockEmailCacheRepository.Setup(x => x.GetExpiredEntriesAsync())
            .ReturnsAsync(new List<EmailCache>());

        // Act
        var result = await _emailCacheService.ProcessExpiredCacheAsync();

        // Assert
        result.Should().Be(0);

        _mockEmailCacheRepository.Verify(x => x.GetExpiredEntriesAsync(), Times.Once);
        _mockEmailCacheRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailCache>()), Times.Never);
        _mockEmailSenderService.Verify(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>()), Times.Never);
    }

    [Fact]
    public async Task ProcessExpiredCacheAsync_SendingFailure_ShouldLogErrorAndContinue()
    {
        // Arrange
        var expiredEntry = new EmailCache
        {
            Id = 1,
            CacheKey = "cache-key-1",
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Subject",
            Body = "Test Body",
            Importance = EmailImportance.Normal,
            MessageCount = 3,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-70),
            UpdatedAt = DateTime.UtcNow.AddMinutes(-70)
        };

        _mockEmailCacheRepository.Setup(x => x.GetExpiredEntriesAsync())
            .ReturnsAsync(new List<EmailCache> { expiredEntry });

        _mockEmailSenderService.Setup(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>()))
            .ThrowsAsync(new Exception("Sending failed"));

        // Act
        var result = await _emailCacheService.ProcessExpiredCacheAsync();

        // Assert
        result.Should().Be(0); // No successful processing due to error

        _mockEmailCacheRepository.Verify(x => x.GetExpiredEntriesAsync(), Times.Once);
        _mockEmailCacheRepository.Verify(x => x.UpdateAsync(It.IsAny<EmailCache>()), Times.Never);
        _mockEmailSenderService.Verify(x => x.SendEmailAsync(It.IsAny<SendEmailRequest>()), Times.Once);
    }
}
