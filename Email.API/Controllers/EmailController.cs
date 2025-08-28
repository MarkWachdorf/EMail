using Email.Application.Services.Interfaces;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Email.API.Controllers;

/// <summary>
/// Controller for managing email messages.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new email message.
    /// </summary>
    /// <param name="request">The email creation request.</param>
    /// <returns>The created email message.</returns>
    /// <response code="201">Email created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<EmailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateEmail([FromBody] CreateEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Creating email for company {CompanyCode}, application {ApplicationCode}",
                request.CompanyCode, request.ApplicationCode);

            var email = await _emailService.CreateEmailAsync(request);

            _logger.LogInformation("Email created successfully with ID {EmailId}", email.Id);

            return CreatedAtAction(nameof(GetEmailById), new { id = email.Id }, 
                BaseResponse<EmailResponse>.Success(email, "Email created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email for company {CompanyCode}", request.CompanyCode);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while creating the email." + ex.Message, "an error occurred while creating the email." + ex.Message));
        }
    }

    /// <summary>
    /// Gets an email message by its ID.
    /// </summary>
    /// <param name="id">The email ID.</param>
    /// <returns>The email message if found.</returns>
    /// <response code="200">Email retrieved successfully.</response>
    /// <response code="404">Email not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(BaseResponse<EmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetEmailById(long id)
    {
        try
        {
            _logger.LogInformation("Retrieving email with ID {EmailId}", id);

            var email = await _emailService.GetEmailByIdAsync(id);

            if (email == null)
            {
                _logger.LogWarning("Email with ID {EmailId} not found", id);
                return NotFound(BaseResponse<object>.Fail($"Email with ID {id} not found.", $"Email with ID {id} not found."));
            }

            _logger.LogInformation("Email with ID {EmailId} retrieved successfully", id);

            return Ok(BaseResponse<EmailResponse>.Success(email, "Email retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email with ID {EmailId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while retrieving the email.", "An error occurred while retrieving the email."));
        }
    }

    /// <summary>
    /// Gets all email messages with optional filtering and pagination.
    /// </summary>
    /// <param name="companyCode">Optional company code filter.</param>
    /// <param name="applicationCode">Optional application code filter.</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10, max: 100).</param>
    /// <returns>Paginated list of email messages.</returns>
    /// <response code="200">Emails retrieved successfully.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<IEnumerable<EmailResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllEmails(
        [FromQuery] string? companyCode,
        [FromQuery] string? applicationCode,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
            {
                return BadRequest(BaseResponse<object>.Fail("Page number must be greater than 0.", "Page number must be greater than 0."));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(BaseResponse<object>.Fail("Page size must be between 1 and 100.", "Page size must be between 1 and 100."));
            }

            _logger.LogInformation("Retrieving emails - Company: {CompanyCode}, App: {ApplicationCode}, Page: {PageNumber}, Size: {PageSize}",
                companyCode ?? "All", applicationCode ?? "All", pageNumber, pageSize);

            var emails = await _emailService.GetAllEmailsAsync(companyCode, applicationCode, pageNumber, pageSize);

            _logger.LogInformation("Retrieved {Count} emails out of {Total} total", 
                emails.Data?.Count() ?? 0, emails.TotalRecords);

            return Ok(emails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving emails");
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while retrieving emails.", "An error occurred while retrieving emails."));
        }
    }

    /// <summary>
    /// Updates the status of an email message.
    /// </summary>
    /// <param name="request">The status update request.</param>
    /// <returns>The updated email message.</returns>
    /// <response code="200">Email status updated successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Email not found.</response>
    /// <response code="409">Concurrency conflict.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPut("status")]
    [ProducesResponseType(typeof(BaseResponse<EmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateEmailStatus([FromBody] UpdateEmailStatusRequest request)
    {
        try
        {
            _logger.LogInformation("Updating status for email {EmailId} to {NewStatus}", 
                request.EmailId, request.NewStatus);

            var email = await _emailService.UpdateEmailStatusAsync(request);

            _logger.LogInformation("Email {EmailId} status updated successfully to {Status}", 
                email.Id, email.Status);

            return Ok(BaseResponse<EmailResponse>.Success(email, "Email status updated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Email with ID {EmailId} not found for status update", request.EmailId);
            return NotFound(BaseResponse<object>.Fail(ex.Message, ex.Message));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency conflict"))
        {
            _logger.LogWarning("Concurrency conflict when updating email {EmailId}", request.EmailId);
            return Conflict(BaseResponse<object>.Fail(ex.Message, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating status for email {EmailId}", request.EmailId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while updating the email status.", "An error occurred while updating the email status."));
        }
    }

    /// <summary>
    /// Soft deletes an email message.
    /// </summary>
    /// <param name="id">The email ID.</param>
    /// <param name="rowVersion">The current row version for optimistic concurrency.</param>
    /// <returns>Success indicator.</returns>
    /// <response code="200">Email deleted successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="404">Email not found.</response>
    /// <response code="409">Concurrency conflict.</response>
    /// <response code="500">Internal server error.</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(typeof(BaseResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteEmail(long id, [FromQuery] byte[] rowVersion)
    {
        try
        {
            if (rowVersion == null || rowVersion.Length == 0)
            {
                return BadRequest(BaseResponse<object>.Fail("Row version is required for optimistic concurrency.", "Row version is required for optimistic concurrency."));
            }

            _logger.LogInformation("Soft deleting email with ID {EmailId}", id);

            var deleted = await _emailService.SoftDeleteEmailAsync(id, rowVersion);

            if (!deleted)
            {
                _logger.LogWarning("Email with ID {EmailId} not found or already deleted", id);
                return NotFound(BaseResponse<object>.Fail($"Email with ID {id} not found or already deleted.", $"Email with ID {id} not found or already deleted."));
            }

            _logger.LogInformation("Email with ID {EmailId} soft deleted successfully", id);

            return Ok(BaseResponse<bool>.Success(true, "Email deleted successfully."));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Concurrency conflict"))
        {
            _logger.LogWarning("Concurrency conflict when deleting email {EmailId}", id);
            return Conflict(BaseResponse<object>.Fail(ex.Message, ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email with ID {EmailId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while deleting the email.", "An error occurred while deleting the email."));
        }
    }
}
