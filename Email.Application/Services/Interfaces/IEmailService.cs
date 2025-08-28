using Email.Contracts.Requests;
using Email.Contracts.Responses;

namespace Email.Application.Services.Interfaces;

/// <summary>
/// Defines the contract for managing email messages.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Creates a new email message record.
    /// </summary>
    /// <param name="request">The request containing email details.</param>
    /// <returns>The created email message response.</returns>
    Task<EmailResponse> CreateEmailAsync(CreateEmailRequest request);

    /// <summary>
    /// Gets an email message by its ID.
    /// </summary>
    /// <param name="id">The ID of the email message.</param>
    /// <returns>The email message response if found, otherwise null.</returns>
    Task<EmailResponse?> GetEmailByIdAsync(long id);

    /// <summary>
    /// Gets all email messages, optionally filtered by company and application code.
    /// </summary>
    /// <param name="companyCode">Optional company code to filter by.</param>
    /// <param name="applicationCode">Optional application code to filter by.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The page size for pagination.</param>
    /// <returns>A paginated list of email message responses.</returns>
    Task<PagedResponse<IEnumerable<EmailResponse>>> GetAllEmailsAsync(string? companyCode, string? applicationCode, int pageNumber, int pageSize);

    /// <summary>
    /// Updates the status of an existing email message.
    /// </summary>
    /// <param name="request">The request containing the email ID, new status, and row version.</param>
    /// <returns>The updated email message response.</returns>
    Task<EmailResponse> UpdateEmailStatusAsync(UpdateEmailStatusRequest request);

    /// <summary>
    /// Soft deletes an email message by its ID.
    /// </summary>
    /// <param name="id">The ID of the email message to delete.</param>
    /// <param name="rowVersion">The current row version for optimistic concurrency.</param>
    /// <returns>True if deletion was successful, false otherwise.</returns>
    Task<bool> SoftDeleteEmailAsync(long id, byte[] rowVersion);
}
