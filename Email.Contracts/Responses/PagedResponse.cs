namespace Email.Contracts.Responses;

/// <summary>
/// Represents a paginated response for standardized API responses.
/// </summary>
/// <typeparam name="T">The type of the data payload (a collection).</typeparam>
public class PagedResponse<T> : BaseResponse<T>
{
    /// <summary>
    /// The current page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// The size of the page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of records across all pages.
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

    /// <summary>
    /// Creates a successful paginated response.
    /// </summary>
    /// <param name="data">The data payload (collection).</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The size of the page.</param>
    /// <param name="totalRecords">The total number of records.</param>
    /// <param name="message">Optional success message.</param>
    /// <returns>A successful PagedResponse.</returns>
    public static PagedResponse<T> Success(T data, int pageNumber, int pageSize, int totalRecords, string? message = null)
    {
        return new PagedResponse<T>
        {
            Succeeded = true,
            Data = data,
            Message = message,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords
        };
    }
}
