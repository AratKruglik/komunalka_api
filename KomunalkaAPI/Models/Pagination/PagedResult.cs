namespace KomunalkaAPI.Models.Pagination;

/// <summary>
/// Paginated query result
/// </summary>
/// <typeparam name="T">Type of items</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// List of items on the current page
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Current page (starts from 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total count of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
}
