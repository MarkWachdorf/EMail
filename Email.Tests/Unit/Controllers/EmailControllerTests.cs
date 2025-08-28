using Email.API.Controllers;
using Email.Application.Services.Interfaces;
using Email.Contracts.Enums;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Email.Tests.Unit.Controllers;

public class EmailControllerTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<EmailController>> _mockLogger;
    private readonly EmailController _emailController;

    public EmailControllerTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<EmailController>>();
        _emailController = new EmailController(_mockEmailService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateEmail_ValidRequest_ShouldReturnCreatedResponse()
    {
        // Arrange
        var request = new CreateEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Email",
            Body = "Test Body",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3
        };

        var emailResponse = new EmailResponse
        {
            Id = 1,
            CompanyCode = request.CompanyCode,
            ApplicationCode = request.ApplicationCode,
            FromAddress = request.FromAddress,
            ToAddresses = request.ToAddresses,
            Subject = request.Subject,
            Body = request.Body,
            Status = EmailStatusDto.Pending,
            Importance = EmailImportanceDto.Normal,
            RetryCount = 0,
            MaxRetries = request.MaxRetries,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailService.Setup(x => x.CreateEmailAsync(request))
            .ReturnsAsync(emailResponse);

        // Act
        var result = await _emailController.CreateEmail(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        createdAtResult!.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdAtResult.ActionName.Should().Be(nameof(EmailController.GetEmailById));
        createdAtResult.RouteValues!["id"].Should().Be(1);

        var response = createdAtResult.Value as BaseResponse<EmailResponse>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(1);
        response.Message.Should().Be("Email created successfully.");

        _mockEmailService.Verify(x => x.CreateEmailAsync(request), Times.Once);
    }

    [Fact]
    public async Task CreateEmail_ServiceThrowsException_ShouldReturnInternalServerError()
    {
        // Arrange
        var request = new CreateEmailRequest
        {
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Email",
            Body = "Test Body",
            Importance = EmailImportanceDto.Normal,
            MaxRetries = 3
        };

        _mockEmailService.Setup(x => x.CreateEmailAsync(request))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _emailController.CreateEmail(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var response = objectResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be("An error occurred while creating the email.");

        _mockEmailService.Verify(x => x.CreateEmailAsync(request), Times.Once);
    }

    [Fact]
    public async Task GetEmailById_ExistingEmail_ShouldReturnOkResponse()
    {
        // Arrange
        var emailId = 1L;
        var emailResponse = new EmailResponse
        {
            Id = emailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Email",
            Body = "Test Body",
            Status = EmailStatusDto.Sent,
            Importance = EmailImportanceDto.Normal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailService.Setup(x => x.GetEmailByIdAsync(emailId))
            .ReturnsAsync(emailResponse);

        // Act
        var result = await _emailController.GetEmailById(emailId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var response = okResult.Value as BaseResponse<EmailResponse>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(emailId);
        response.Message.Should().Be("Email retrieved successfully.");

        _mockEmailService.Verify(x => x.GetEmailByIdAsync(emailId), Times.Once);
    }

    [Fact]
    public async Task GetEmailById_NonExistingEmail_ShouldReturnNotFound()
    {
        // Arrange
        var emailId = 999L;

        _mockEmailService.Setup(x => x.GetEmailByIdAsync(emailId))
            .ReturnsAsync((EmailResponse?)null);

        // Act
        var result = await _emailController.GetEmailById(emailId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var response = notFoundResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be($"Email with ID {emailId} not found.");

        _mockEmailService.Verify(x => x.GetEmailByIdAsync(emailId), Times.Once);
    }

    [Fact]
    public async Task GetAllEmails_ValidRequest_ShouldReturnOkResponse()
    {
        // Arrange
        var companyCode = "TEST";
        var applicationCode = "APP1";
        var pageNumber = 1;
        var pageSize = 10;

        var emails = new List<EmailResponse>
        {
            new EmailResponse
            {
                Id = 1,
                CompanyCode = companyCode,
                ApplicationCode = applicationCode,
                FromAddress = "test@example.com",
                ToAddresses = "wachdorfm@hotmail.com",
                Subject = "Test Email 1",
                Body = "Test Body 1",
                Status = EmailStatusDto.Sent,
                Importance = EmailImportanceDto.Normal,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new EmailResponse
            {
                Id = 2,
                CompanyCode = companyCode,
                ApplicationCode = applicationCode,
                FromAddress = "test@example.com",
                ToAddresses = "user2@example.com",
                Subject = "Test Email 2",
                Body = "Test Body 2",
                Status = EmailStatusDto.Pending,
                Importance = EmailImportanceDto.High,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        var pagedResponse = PagedResponse<IEnumerable<EmailResponse>>.Success(
            emails, pageNumber, pageSize, 2, "Emails retrieved successfully.");

        _mockEmailService.Setup(x => x.GetAllEmailsAsync(companyCode, applicationCode, pageNumber, pageSize))
            .ReturnsAsync(pagedResponse);

        // Act
        var result = await _emailController.GetAllEmails(companyCode, applicationCode, pageNumber, pageSize);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var response = okResult.Value as PagedResponse<IEnumerable<EmailResponse>>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Should().HaveCount(2);
        response.TotalRecords.Should().Be(2);
        response.PageNumber.Should().Be(pageNumber);
        response.PageSize.Should().Be(pageSize);

        _mockEmailService.Verify(x => x.GetAllEmailsAsync(companyCode, applicationCode, pageNumber, pageSize), Times.Once);
    }

    [Fact]
    public async Task GetAllEmails_InvalidPageNumber_ShouldReturnBadRequest()
    {
        // Arrange
        var pageNumber = 0; // Invalid
        var pageSize = 10;

        // Act
        var result = await _emailController.GetAllEmails(null, null, pageNumber, pageSize);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var response = badRequestResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be("Page number must be greater than 0.");

        _mockEmailService.Verify(x => x.GetAllEmailsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetAllEmails_InvalidPageSize_ShouldReturnBadRequest()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 101; // Invalid (exceeds max of 100)

        // Act
        var result = await _emailController.GetAllEmails(null, null, pageNumber, pageSize);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var response = badRequestResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be("Page size must be between 1 and 100.");

        _mockEmailService.Verify(x => x.GetAllEmailsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task UpdateEmailStatus_ValidRequest_ShouldReturnOkResponse()
    {
        // Arrange
        var request = new UpdateEmailStatusRequest
        {
            EmailId = 1,
            NewStatus = EmailStatusDto.Sent,
            StatusMessage = "Email sent successfully",
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };

        var emailResponse = new EmailResponse
        {
            Id = request.EmailId,
            CompanyCode = "TEST",
            ApplicationCode = "APP1",
            FromAddress = "test@example.com",
            ToAddresses = "wachdorfm@hotmail.com",
            Subject = "Test Email",
            Body = "Test Body",
            Status = request.NewStatus,
            StatusMessage = request.StatusMessage,
            Importance = EmailImportanceDto.Normal,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _mockEmailService.Setup(x => x.UpdateEmailStatusAsync(request))
            .ReturnsAsync(emailResponse);

        // Act
        var result = await _emailController.UpdateEmailStatus(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var response = okResult.Value as BaseResponse<EmailResponse>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().Be(request.EmailId);
        response.Data.Status.Should().Be(request.NewStatus);
        response.Message.Should().Be("Email status updated successfully.");

        _mockEmailService.Verify(x => x.UpdateEmailStatusAsync(request), Times.Once);
    }

    [Fact]
    public async Task UpdateEmailStatus_EmailNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var request = new UpdateEmailStatusRequest
        {
            EmailId = 999,
            NewStatus = EmailStatusDto.Sent,
            StatusMessage = "Email sent successfully",
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };

        _mockEmailService.Setup(x => x.UpdateEmailStatusAsync(request))
            .ThrowsAsync(new KeyNotFoundException($"Email with ID {request.EmailId} not found or is deleted."));

        // Act
        var result = await _emailController.UpdateEmailStatus(request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var response = notFoundResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be($"Email with ID {request.EmailId} not found or is deleted.");

        _mockEmailService.Verify(x => x.UpdateEmailStatusAsync(request), Times.Once);
    }

    [Fact]
    public async Task UpdateEmailStatus_ConcurrencyConflict_ShouldReturnConflict()
    {
        // Arrange
        var request = new UpdateEmailStatusRequest
        {
            EmailId = 1,
            NewStatus = EmailStatusDto.Sent,
            StatusMessage = "Email sent successfully",
            RowVersion = new byte[] { 1, 2, 3, 4 }
        };

        _mockEmailService.Setup(x => x.UpdateEmailStatusAsync(request))
            .ThrowsAsync(new InvalidOperationException("Concurrency conflict: Email has been modified by another process."));

        // Act
        var result = await _emailController.UpdateEmailStatus(request);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;
        conflictResult!.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var response = conflictResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be("Concurrency conflict: Email has been modified by another process.");

        _mockEmailService.Verify(x => x.UpdateEmailStatusAsync(request), Times.Once);
    }

    [Fact]
    public async Task DeleteEmail_ValidRequest_ShouldReturnOkResponse()
    {
        // Arrange
        var emailId = 1L;
        var rowVersion = new byte[] { 1, 2, 3, 4 };

        _mockEmailService.Setup(x => x.SoftDeleteEmailAsync(emailId, rowVersion))
            .ReturnsAsync(true);

        // Act
        var result = await _emailController.DeleteEmail(emailId, rowVersion);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(StatusCodes.Status200OK);

        var response = okResult.Value as BaseResponse<bool>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeTrue();
        response.Data.Should().BeTrue();
        response.Message.Should().Be("Email deleted successfully.");

        _mockEmailService.Verify(x => x.SoftDeleteEmailAsync(emailId, rowVersion), Times.Once);
    }

    [Fact]
    public async Task DeleteEmail_EmailNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var emailId = 999L;
        var rowVersion = new byte[] { 1, 2, 3, 4 };

        _mockEmailService.Setup(x => x.SoftDeleteEmailAsync(emailId, rowVersion))
            .ReturnsAsync(false);

        // Act
        var result = await _emailController.DeleteEmail(emailId, rowVersion);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var response = notFoundResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be($"Email with ID {emailId} not found or already deleted.");

        _mockEmailService.Verify(x => x.SoftDeleteEmailAsync(emailId, rowVersion), Times.Once);
    }

    [Fact]
    public async Task DeleteEmail_ConcurrencyConflict_ShouldReturnConflict()
    {
        // Arrange
        var emailId = 1L;
        var rowVersion = new byte[] { 1, 2, 3, 4 };

        _mockEmailService.Setup(x => x.SoftDeleteEmailAsync(emailId, rowVersion))
            .ThrowsAsync(new InvalidOperationException("Concurrency conflict: Email has been modified by another process."));

        // Act
        var result = await _emailController.DeleteEmail(emailId, rowVersion);

        // Assert
        result.Should().BeOfType<ConflictObjectResult>();
        var conflictResult = result as ConflictObjectResult;
        conflictResult!.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var response = conflictResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be("Concurrency conflict: Email has been modified by another process.");

        _mockEmailService.Verify(x => x.SoftDeleteEmailAsync(emailId, rowVersion), Times.Once);
    }

    [Fact]
    public async Task DeleteEmail_MissingRowVersion_ShouldReturnBadRequest()
    {
        // Arrange
        var emailId = 1L;
        byte[]? rowVersion = null;

        // Act
        var result = await _emailController.DeleteEmail(emailId, rowVersion);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var response = badRequestResult.Value as BaseResponse<object>;
        response.Should().NotBeNull();
        response!.Succeeded.Should().BeFalse();
        response.Message.Should().Be("Row version is required for optimistic concurrency.");

        _mockEmailService.Verify(x => x.SoftDeleteEmailAsync(It.IsAny<long>(), It.IsAny<byte[]>()), Times.Never);
    }
}
