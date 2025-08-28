using Email.Application.Services.Interfaces;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Email.API.Controllers;

/// <summary>
/// Controller for sending email messages.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmailSenderController : ControllerBase
{
    private readonly IEmailSenderService _emailSenderService;
    private readonly ILogger<EmailSenderController> _logger;

    public EmailSenderController(IEmailSenderService emailSenderService, ILogger<EmailSenderController> logger)
    {
        _emailSenderService = emailSenderService ?? throw new ArgumentNullException(nameof(emailSenderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends an email immediately.
    /// </summary>
    /// <param name="request">The email sending request.</param>
    /// <returns>The email message with sending status.</returns>
    /// <response code="200">Email sent successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("send")]
    [ProducesResponseType(typeof(BaseResponse<EmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Sending email for company {CompanyCode}, application {ApplicationCode} to {ToAddresses}",
                request.CompanyCode, request.ApplicationCode, request.ToAddresses);

            var email = await _emailSenderService.SendEmailAsync(request);

            _logger.LogInformation("Email sent successfully with ID {EmailId}, status: {Status}", 
                email.Id, email.Status);

            return Ok(BaseResponse<EmailResponse>.Success(email, "Email sent successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email for company {CompanyCode}", request.CompanyCode);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while sending the email."));
        }
    }

    /// <summary>
    /// Processes and sends a batch of pending emails.
    /// </summary>
    /// <param name="companyCode">Optional company code filter.</param>
    /// <param name="applicationCode">Optional application code filter.</param>
    /// <returns>The number of emails processed and sent.</returns>
    /// <response code="200">Batch processing completed successfully.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("process-pending")]
    [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessPendingEmails(
        [FromQuery] string? companyCode = null,
        [FromQuery] string? applicationCode = null)
    {
        try
        {
            _logger.LogInformation("Processing pending emails - Company: {CompanyCode}, App: {ApplicationCode}",
                companyCode ?? "All", applicationCode ?? "All");

            var processedCount = await _emailSenderService.ProcessPendingEmailsAsync(companyCode, applicationCode);

            _logger.LogInformation("Processed {Count} pending emails successfully", processedCount);

            return Ok(BaseResponse<int>.Success(processedCount, 
                $"Successfully processed {processedCount} pending emails."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing pending emails");
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while processing pending emails."));
        }
    }

    /// <summary>
    /// Retries sending a specific failed email.
    /// </summary>
    /// <param name="emailId">The ID of the email to retry.</param>
    /// <returns>The updated email message.</returns>
    /// <response code="200">Email retry initiated successfully.</response>
    /// <response code="400">Email is not in failed status.</response>
    /// <response code="404">Email not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("{emailId:long}/retry")]
    [ProducesResponseType(typeof(BaseResponse<EmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RetryFailedEmail(long emailId)
    {
        try
        {
            _logger.LogInformation("Retrying failed email with ID {EmailId}", emailId);

            var email = await _emailSenderService.RetryFailedEmailAsync(emailId);

            _logger.LogInformation("Email {EmailId} retry initiated successfully, status: {Status}", 
                email.Id, email.Status);

            return Ok(BaseResponse<EmailResponse>.Success(email, "Email retry initiated successfully."));
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Email with ID {EmailId} not found for retry", emailId);
            return NotFound(BaseResponse<object>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Cannot retry email {EmailId}: {Message}", emailId, ex.Message);
            return BadRequest(BaseResponse<object>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying email with ID {EmailId}", emailId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while retrying the email."));
        }
    }
}
