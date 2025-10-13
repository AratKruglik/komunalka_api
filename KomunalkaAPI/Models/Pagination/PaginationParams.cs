using System.ComponentModel.DataAnnotations;

namespace KomunalkaAPI.Models.Pagination;

/// <summary>
/// Параметри пагінації
/// </summary>
public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    /// <summary>
    /// Номер сторінки (починається з 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Номер сторінки повинен бути більше 0")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Кількість елементів на сторінці (максимум 100)
    /// </summary>
    [Range(1, MaxPageSize, ErrorMessage = "Розмір сторінки повинен бути від 1 до 100")]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
}
