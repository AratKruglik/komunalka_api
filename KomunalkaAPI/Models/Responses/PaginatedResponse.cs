namespace KomunalkaAPI.Models.Responses;

/// <summary>
/// Paginated API response (Laravel-compatible format)
/// Replicates Laravel Eloquent API Resources structure for paginated data
/// </summary>
/// <typeparam name="T">Type of collection items</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Current page data
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Navigation links between pages
    /// </summary>
    public PaginationLinks Links { get; set; } = new();

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public PaginationMeta Meta { get; set; } = new();
}

/// <summary>
/// Navigation links between pages
/// </summary>
public class PaginationLinks
{
    /// <summary>
    /// Link to the first page
    /// </summary>
    public string? First { get; set; }

    /// <summary>
    /// Link to the last page
    /// </summary>
    public string? Last { get; set; }

    /// <summary>
    /// Link to the previous page (null if this is the first page)
    /// </summary>
    public string? Prev { get; set; }

    /// <summary>
    /// Link to the next page (null if this is the last page)
    /// </summary>
    public string? Next { get; set; }
}

/// <summary>
/// Pagination metadata
/// </summary>
public class PaginationMeta
{
    /// <summary>
    /// Current page
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Index of the first item on the current page (1-based)
    /// </summary>
    public int? From { get; set; }

    /// <summary>
    /// Last page number
    /// </summary>
    public int LastPage { get; set; }

    /// <summary>
    /// Resource path (base URL without query parameters)
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PerPage { get; set; }

    /// <summary>
    /// Index of the last item on the current page (1-based)
    /// </summary>
    public int? To { get; set; }

    /// <summary>
    /// Total count of items
    /// </summary>
    public int Total { get; set; }
}
