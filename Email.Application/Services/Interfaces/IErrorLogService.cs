using Email.Contracts.Requests;
using Email.Contracts.Responses;

namespace Email.Application.Services.Interfaces;

/// <summary>
/// Defines the contract for logging errors.
/// </summary>
public interface IErrorLogService
{
    /// <summary>
    /// Logs an error entry.
    /// </summary>
    /// <param name="request">The request containing error details.</param>
    /// <returns>The created error log response.</returns>
    Task<ErrorLogResponse> LogErrorAsync(LogErrorRequest request);

    /// <summary>
    /// Gets an error log entry by its ID.
    /// </summary>
    /// <param name="id">The ID of the error log entry.</param>
    /// <returns>The error log response if found, otherwise null.</returns>
    Task<ErrorLogResponse?> GetErrorLogByIdAsync(long id);

    /// <summary>
    /// Gets all error log entries, optionally filtered by level, company, or application code.
    /// </summary>
    /// <param name="level">Optional log level to filter by.</param>
    /// <param name="companyCode">Optional company code to filter by.</param>
    /// <param name="applicationCode">Optional application code to filter by.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The page size for pagination.</param>
    /// <returns>A paginated list of error log responses.</returns>
    Task<PagedResponse<IEnumerable<ErrorLogResponse>>> GetAllErrorLogsAsync(string? level, string? companyCode, string? applicationCode, int pageNumber, int pageSize);
}
