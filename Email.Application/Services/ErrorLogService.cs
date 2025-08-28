using Email.Application.Mappers;
using Email.Application.Services.Interfaces;
using Email.Contracts.Requests;
using Email.Contracts.Responses;
using Email.Infrastructure.Repositories.Interfaces;

namespace Email.Application.Services;

/// <summary>
/// Service for logging and retrieving error entries.
/// </summary>
public class ErrorLogService : IErrorLogService
{
    private readonly IErrorLogRepository _errorLogRepository;

    public ErrorLogService(IErrorLogRepository errorLogRepository)
    {
        _errorLogRepository = errorLogRepository ?? throw new ArgumentNullException(nameof(errorLogRepository));
    }

    /// <inheritdoc />
    public async Task<ErrorLogResponse> LogErrorAsync(LogErrorRequest request)
    {
        var errorLog = request.ToEntity();
        var createdErrorLog = await _errorLogRepository.AddAsync(errorLog);
        return createdErrorLog.ToResponse();
    }

    /// <inheritdoc />
    public async Task<ErrorLogResponse?> GetErrorLogByIdAsync(long id)
    {
        var errorLog = await _errorLogRepository.GetByIdAsync(id);
        return errorLog?.ToResponse();
    }

    /// <inheritdoc />
    public async Task<PagedResponse<IEnumerable<ErrorLogResponse>>> GetAllErrorLogsAsync(string? level, string? companyCode, string? applicationCode, int pageNumber, int pageSize)
    {
        // For simplicity, this example fetches all and then filters/pages in memory.
        // In a real-world scenario, this would be pushed to the repository for efficient database querying.
        var allErrorLogs = await _errorLogRepository.GetAllAsync();

        var filteredErrorLogs = allErrorLogs
            .Where(e => (string.IsNullOrEmpty(level) || e.Level.ToString().Equals(level, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(companyCode) || e.CompanyCode.Equals(companyCode, StringComparison.OrdinalIgnoreCase)) &&
                        (string.IsNullOrEmpty(applicationCode) || e.ApplicationCode.Equals(applicationCode, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        var totalRecords = filteredErrorLogs.Count;
        var pagedErrorLogs = filteredErrorLogs
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return PagedResponse<IEnumerable<ErrorLogResponse>>.Success(
            pagedErrorLogs.Select(e => e.ToResponse()),
            pageNumber,
            pageSize,
            totalRecords,
            "Error logs retrieved successfully."
        );
    }
}
