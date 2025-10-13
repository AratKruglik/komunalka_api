using KomunalkaAPI.Models.Pagination;
using KomunalkaAPI.Models.Responses;
using Microsoft.AspNetCore.Http;

namespace KomunalkaAPI.Extensions;

/// <summary>
/// Extension methods for converting pagination to Laravel-compatible format
/// </summary>
public static class PaginationExtensions
{
    /// <summary>
    /// Converts PagedResult to paginated response (Laravel-compatible format)
    /// </summary>
    public static PaginatedResponse<TDto> ToPaginatedResponse<TEntity, TDto>(
        this PagedResult<TEntity> pagedResult,
        List<TDto> mappedData,
        HttpRequest request)
    {
        var baseUrl = $"{request.Scheme}://{request.Host}{request.Path}";

        return new PaginatedResponse<TDto>
        {
            Data = mappedData,
            Links = new PaginationLinks
            {
                First = BuildPageUrl(baseUrl, 1),
                Last = BuildPageUrl(baseUrl, pagedResult.TotalPages),
                Prev = pagedResult.HasPrevious ? BuildPageUrl(baseUrl, pagedResult.PageNumber - 1) : null,
                Next = pagedResult.HasNext ? BuildPageUrl(baseUrl, pagedResult.PageNumber + 1) : null
            },
            Meta = new PaginationMeta
            {
                CurrentPage = pagedResult.PageNumber,
                From = pagedResult.Items.Count > 0 ? (pagedResult.PageNumber - 1) * pagedResult.PageSize + 1 : null,
                LastPage = pagedResult.TotalPages,
                Path = baseUrl,
                PerPage = pagedResult.PageSize,
                To = pagedResult.Items.Count > 0 ? (pagedResult.PageNumber - 1) * pagedResult.PageSize + pagedResult.Items.Count : null,
                Total = pagedResult.TotalCount
            }
        };
    }

    /// <summary>
    /// Converts PagedResult to paginated response with automatic mapping
    /// </summary>
    public static PaginatedResponse<T> ToPaginatedResponse<T>(
        this PagedResult<T> pagedResult,
        HttpRequest request)
    {
        var baseUrl = $"{request.Scheme}://{request.Host}{request.Path}";

        return new PaginatedResponse<T>
        {
            Data = pagedResult.Items.ToList(),
            Links = new PaginationLinks
            {
                First = BuildPageUrl(baseUrl, 1),
                Last = BuildPageUrl(baseUrl, pagedResult.TotalPages),
                Prev = pagedResult.HasPrevious ? BuildPageUrl(baseUrl, pagedResult.PageNumber - 1) : null,
                Next = pagedResult.HasNext ? BuildPageUrl(baseUrl, pagedResult.PageNumber + 1) : null
            },
            Meta = new PaginationMeta
            {
                CurrentPage = pagedResult.PageNumber,
                From = pagedResult.Items.Count > 0 ? (pagedResult.PageNumber - 1) * pagedResult.PageSize + 1 : null,
                LastPage = pagedResult.TotalPages,
                Path = baseUrl,
                PerPage = pagedResult.PageSize,
                To = pagedResult.Items.Count > 0 ? (pagedResult.PageNumber - 1) * pagedResult.PageSize + pagedResult.Items.Count : null,
                Total = pagedResult.TotalCount
            }
        };
    }

    private static string BuildPageUrl(string baseUrl, int pageNumber)
    {
        return $"{baseUrl}?page={pageNumber}";
    }
}
