using Email.Application.Services.Interfaces;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Email.API.Controllers;

/// <summary>
/// Controller for managing cached email messages for consolidation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmailCacheController : ControllerBase
{
    private readonly IEmailCacheService _emailCacheService;
    private readonly ILogger<EmailCacheController> _logger;

    public EmailCacheController(IEmailCacheService emailCacheService, ILogger<EmailCacheController> logger)
    {
        _emailCacheService = emailCacheService ?? throw new ArgumentNullException(nameof(emailCacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Sends an email that might be cached for consolidation.
    /// </summary>
    /// <param name="request">The cached email sending request.</param>
    /// <returns>The email message (could be cached or sent immediately).</returns>
    /// <response code="200">Email processed successfully (cached or sent).</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("send-cached")]
    [ProducesResponseType(typeof(BaseResponse<EmailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SendCachedEmail([FromBody] SendCachedEmailRequest request)
    {
        try
        {
            _logger.LogInformation("Processing cached email for company {CompanyCode}, application {ApplicationCode} to {ToAddresses}",
                request.CompanyCode, request.ApplicationCode, request.ToAddresses);

            var email = await _emailCacheService.SendCachedEmailAsync(request);

            var message = email.Status == Email.Contracts.Enums.EmailStatusDto.Cached 
                ? "Email cached for consolidation." 
                : "Email sent immediately.";

            _logger.LogInformation("Cached email processed successfully with ID {EmailId}, status: {Status}", 
                email.Id, email.Status);

            return Ok(BaseResponse<EmailResponse>.Success(email, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing cached email for company {CompanyCode}", request.CompanyCode);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while processing the cached email."));
        }
    }

    /// <summary>
    /// Processes expired cached emails, consolidating and sending them.
    /// </summary>
    /// <returns>The number of cached emails processed and sent.</returns>
    /// <response code="200">Cache processing completed successfully.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("process-expired")]
    [ProducesResponseType(typeof(BaseResponse<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ProcessExpiredCache()
    {
        try
        {
            _logger.LogInformation("Processing expired cache entries");

            var processedCount = await _emailCacheService.ProcessExpiredCacheAsync();

            _logger.LogInformation("Processed {Count} expired cache entries successfully", processedCount);

            return Ok(BaseResponse<int>.Success(processedCount, 
                $"Successfully processed {processedCount} expired cache entries."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing expired cache entries");
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while processing expired cache entries."));
        }
    }
}
