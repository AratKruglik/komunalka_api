namespace KomunalkaAPI.Models.Responses;

public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public PaginationLinks Links { get; set; } = new();
    public PaginationMeta Meta { get; set; } = new();
}

public class PaginationLinks
{
    public string? First { get; set; }
    public string? Last { get; set; }
    public string? Prev { get; set; }
    public string? Next { get; set; }
}

public class PaginationMeta
{
    public int CurrentPage { get; set; }
    public int? From { get; set; }
    public int LastPage { get; set; }
    public string Path { get; set; } = string.Empty;
    public int PerPage { get; set; }
    public int? To { get; set; }
    public int Total { get; set; }
}
