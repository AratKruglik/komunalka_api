namespace KomunalkaAPI.Models.Pagination;

/// <summary>
/// Результат запиту з пагінацією
/// </summary>
/// <typeparam name="T">Тип елементів</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// Список елементів на поточній сторінці
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Поточна сторінка (починається з 1)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Кількість елементів на сторінці
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Загальна кількість елементів
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Загальна кількість сторінок
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Чи є попередня сторінка
    /// </summary>
    public bool HasPrevious => PageNumber > 1;

    /// <summary>
    /// Чи є наступна сторінка
    /// </summary>
    public bool HasNext => PageNumber < TotalPages;
}
