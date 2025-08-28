using Email.Application.Services.Interfaces;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Microsoft.AspNetCore.Mvc;

namespace Email.API.Controllers;

/// <summary>
/// Controller for managing error logs.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ErrorLogController : ControllerBase
{
    private readonly IErrorLogService _errorLogService;
    private readonly ILogger<ErrorLogController> _logger;

    public ErrorLogController(IErrorLogService errorLogService, ILogger<ErrorLogController> logger)
    {
        _errorLogService = errorLogService ?? throw new ArgumentNullException(nameof(errorLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Logs an error entry.
    /// </summary>
    /// <param name="request">The error logging request.</param>
    /// <returns>The created error log entry.</returns>
    /// <response code="201">Error logged successfully.</response>
    /// <response code="400">Invalid request data.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(BaseResponse<ErrorLogResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LogError([FromBody] LogErrorRequest request)
    {
        try
        {
            _logger.LogInformation("Logging error for source {Source}, level {Level}", 
                request.Source, request.Level);

            var errorLog = await _errorLogService.LogErrorAsync(request);

            _logger.LogInformation("Error logged successfully with ID {ErrorLogId}", errorLog.Id);

            return CreatedAtAction(nameof(GetErrorLogById), new { id = errorLog.Id }, 
                BaseResponse<ErrorLogResponse>.Success(errorLog, "Error logged successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging error entry for source {Source}", request.Source);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while logging the error."));
        }
    }

    /// <summary>
    /// Gets an error log entry by its ID.
    /// </summary>
    /// <param name="id">The error log ID.</param>
    /// <returns>The error log entry if found.</returns>
    /// <response code="200">Error log retrieved successfully.</response>
    /// <response code="404">Error log not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(BaseResponse<ErrorLogResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetErrorLogById(long id)
    {
        try
        {
            _logger.LogInformation("Retrieving error log with ID {ErrorLogId}", id);

            var errorLog = await _errorLogService.GetErrorLogByIdAsync(id);

            if (errorLog == null)
            {
                _logger.LogWarning("Error log with ID {ErrorLogId} not found", id);
                return NotFound(BaseResponse<object>.Fail($"Error log with ID {id} not found."));
            }

            _logger.LogInformation("Error log with ID {ErrorLogId} retrieved successfully", id);

            return Ok(BaseResponse<ErrorLogResponse>.Success(errorLog, "Error log retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving error log with ID {ErrorLogId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while retrieving the error log."));
        }
    }

    /// <summary>
    /// Gets all error log entries with optional filtering and pagination.
    /// </summary>
    /// <param name="level">Optional log level filter.</param>
    /// <param name="companyCode">Optional company code filter.</param>
    /// <param name="applicationCode">Optional application code filter.</param>
    /// <param name="pageNumber">Page number (default: 1).</param>
    /// <param name="pageSize">Page size (default: 10, max: 100).</param>
    /// <returns>Paginated list of error log entries.</returns>
    /// <response code="200">Error logs retrieved successfully.</response>
    /// <response code="400">Invalid pagination parameters.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<IEnumerable<ErrorLogResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllErrorLogs(
        [FromQuery] string? level,
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
                return BadRequest(BaseResponse<object>.Fail("Page number must be greater than 0."));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(BaseResponse<object>.Fail("Page size must be between 1 and 100."));
            }

            _logger.LogInformation("Retrieving error logs - Level: {Level}, Company: {CompanyCode}, App: {ApplicationCode}, Page: {PageNumber}, Size: {PageSize}",
                level ?? "All", companyCode ?? "All", applicationCode ?? "All", pageNumber, pageSize);

            var errorLogs = await _errorLogService.GetAllErrorLogsAsync(level, companyCode, applicationCode, pageNumber, pageSize);

            _logger.LogInformation("Retrieved {Count} error logs out of {Total} total", 
                errorLogs.Data?.Count() ?? 0, errorLogs.TotalRecords);

            return Ok(errorLogs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving error logs");
            return StatusCode(StatusCodes.Status500InternalServerError,
                BaseResponse<object>.Fail("An error occurred while retrieving error logs."));
        }
    }
}
