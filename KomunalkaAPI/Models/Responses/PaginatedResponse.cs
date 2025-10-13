namespace KomunalkaAPI.Models.Responses;

/// <summary>
/// Paginated API response (Laravel-compatible format)
/// Повторює структуру Laravel Eloquent API Resources для пагінованих даних
/// </summary>
/// <typeparam name="T">Тип елементів колекції</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// Дані поточної сторінки
    /// </summary>
    public List<T> Data { get; set; } = new();

    /// <summary>
    /// Посилання для навігації по сторінках
    /// </summary>
    public PaginationLinks Links { get; set; } = new();

    /// <summary>
    /// Метадані пагінації
    /// </summary>
    public PaginationMeta Meta { get; set; } = new();
}

/// <summary>
/// Посилання для навігації по сторінках
/// </summary>
public class PaginationLinks
{
    /// <summary>
    /// Посилання на першу сторінку
    /// </summary>
    public string? First { get; set; }

    /// <summary>
    /// Посилання на останню сторінку
    /// </summary>
    public string? Last { get; set; }

    /// <summary>
    /// Посилання на попередню сторінку (null якщо це перша сторінка)
    /// </summary>
    public string? Prev { get; set; }

    /// <summary>
    /// Посилання на наступну сторінку (null якщо це остання сторінка)
    /// </summary>
    public string? Next { get; set; }
}

/// <summary>
/// Метадані пагінації
/// </summary>
public class PaginationMeta
{
    /// <summary>
    /// Поточна сторінка
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Індекс першого елемента на поточній сторінці (1-based)
    /// </summary>
    public int? From { get; set; }

    /// <summary>
    /// Номер останньої сторінки
    /// </summary>
    public int LastPage { get; set; }

    /// <summary>
    /// Шлях до ресурсу (базовий URL без query параметрів)
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Кількість елементів на сторінці
    /// </summary>
    public int PerPage { get; set; }

    /// <summary>
    /// Індекс останнього елемента на поточній сторінці (1-based)
    /// </summary>
    public int? To { get; set; }

    /// <summary>
    /// Загальна кількість елементів
    /// </summary>
    public int Total { get; set; }
}
