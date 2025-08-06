# Керівництво з ефективної розробки в Komunalka API

Цей документ містить рекомендації та практики для ефективної розробки в проекті Komunalka API.

## Технічний стек

- **Фреймворк**: ASP.NET Core (.NET 9.0)
- **Мова програмування**: C# 13.0
- **База даних**: PostgreSQL
- **ORM**: Entity Framework Core 9.0
- **Контейнеризація**: Docker

## Організація коду

### Структура проекту

Дотримуйтесь чіткої структури проекту:

- **Controllers/**: REST API контролери
- **Models/**: Класи моделей даних
- **DTO/**: Об'єкти передачі даних
- **Data/**: Контекст бази даних та міграції
- **Repositories/**: Репозиторії для роботи з даними
- **Migrations/**: Міграції Entity Framework
- **Services/**: Бізнес-логіка

### Найменування

- Використовуйте **PascalCase** для:
  - Назв класів
  - Інтерфейсів (починаються з `I`)
  - Методів
  - Властивостей
  - Enum
- Використовуйте **camelCase** для:
  - Параметрів методів
  - Локальних змінних

### Приклади найменувань

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> GetUserByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return MapToDto(user);
    }
}
```

## Коментарі та документація

- Документуйте публічні API за допомогою XML-коментарів
- Пишіть коментарі для складних алгоритмів і бізнес-логіки
- Оновлюйте README.md при внесенні значних змін

```csharp
/// <summary>
/// Отримує користувача за його ідентифікатором
/// </summary>
/// <param name="userId">Ідентифікатор користувача</param>
/// <returns>DTO користувача або null, якщо користувача не знайдено</returns>
public async Task<UserDto> GetUserByIdAsync(int userId)
```

## Робота з Entity Framework

### Міграції

- Створюйте окремі міграції для кожної логічної зміни схеми
- Використовуйте змістовні назви міграцій
- Перевіряйте згенеровані міграції перед застосуванням

```bash
dotnet ef migrations add AddUserRoles
dotnet ef database update
```

### Оптимізація запитів

- Використовуйте `Include()` для eager loading пов'язаних сутностей
- Застосовуйте `AsNoTracking()` для запитів тільки для читання
- Використовуйте проекції для отримання лише необхідних полів

```csharp
var users = await _context.Users
    .AsNoTracking()
    .Include(u => u.Addresses)
    .Select(u => new UserDto { Id = u.Id, Name = u.Name })
    .ToListAsync();
```

## Обробка помилок

- Використовуйте глобальний обробник винятків для API
- Створіть спеціалізовані класи винятків для різних ситуацій
- Логуйте помилки з достатнім контекстом для діагностики

```csharp
try
{
    // Бізнес-логіка
}
catch (EntityNotFoundException ex)
{
    _logger.LogWarning(ex, "Сутність не знайдена: {EntityId}", ex.EntityId);
    return NotFound(new ErrorResponse(ex.Message));
}
catch (Exception ex)
{
    _logger.LogError(ex, "Неочікувана помилка при обробці запиту");
    return StatusCode(500, new ErrorResponse("Внутрішня помилка сервера"));
}
```

## Асинхронне програмування

- Використовуйте асинхронні методи при роботі з I/O операціями
- Застосовуйте суфікс `Async` для асинхронних методів
- Уникайте блокуючих викликів у асинхронному коді

```csharp
// Правильно
public async Task<User> GetUserAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// Неправильно
public async Task<User> GetUserAsync(int id)
{
    return _repository.GetById(id); // Блокуючий виклик
}
```

## Тестування

### Юніт-тести

- Пишіть тести для всіх важливих компонентів бізнес-логіки
- Використовуйте моки для залежностей
- Дотримуйтесь патерну AAA (Arrange-Act-Assert)

```csharp
[Fact]
public async Task GetUserById_WithValidId_ReturnsUser()
{
    // Arrange
    var userId = 1;
    var mockUser = new User { Id = userId, Name = "Test User" };
    _mockRepository.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(mockUser);

    // Act
    var result = await _userService.GetUserByIdAsync(userId);

    // Assert
    Assert.NotNull(result);
    Assert.Equal(userId, result.Id);
    Assert.Equal("Test User", result.Name);
}
```

### Інтеграційні тести

- Використовуйте тестову базу даних
- Налаштовуйте тестові дані перед кожним тестом
- Очищайте тестові дані після тестів

## Безпека

- Використовуйте параметризовані запити для запобігання SQL-ін'єкцій
- Валідуйте всі вхідні дані від користувачів
- Хешуйте паролі з використанням сучасних алгоритмів
- Налаштуйте CORS правильно

```csharp
// Налаштування CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://trusted-site.com")
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});
```

## Продуктивність

- Використовуйте кешування для частих запитів
- Оптимізуйте запити до бази даних
- Уникайте N+1 проблеми при завантаженні пов'язаних сутностей
- Використовуйте пагінацію для великих наборів даних

```csharp
// Кешування
builder.Services.AddMemoryCache();

// Використання кешу
public async Task<List<UserDto>> GetAllUsersAsync()
{
    if (!_memoryCache.TryGetValue("AllUsers", out List<UserDto> users))
    {
        users = await _userRepository.GetAllAsync();
        _memoryCache.Set("AllUsers", users, TimeSpan.FromMinutes(10));
    }
    return users;
}

// Пагінація
public async Task<PagedResult<UserDto>> GetUsersPagedAsync(int pageNumber, int pageSize)
{
    var users = await _context.Users
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var totalCount = await _context.Users.CountAsync();

    return new PagedResult<UserDto>
    {
        Items = users.Select(u => MapToDto(u)).ToList(),
        TotalCount = totalCount,
        PageNumber = pageNumber,
        PageSize = pageSize
    };
}
```

## Розгортання

- Використовуйте Docker для контейнеризації
- Автоматизуйте процес розгортання з CI/CD
- Створіть скрипти для міграції бази даних при розгортанні

## Версіонування API

- Використовуйте семантичне версіонування
- Підтримуйте зворотну сумісність між версіями
- Документуйте зміни API в CHANGELOG.md

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1.0")]
public class UsersController : ControllerBase
{
    // Реалізація API v1
}

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("2.0")]
public class UsersV2Controller : ControllerBase
{
    // Реалізація API v2
}
```

## Моніторинг та логування

- Налаштуйте структуроване логування з використанням Serilog
- Встановіть відповідні рівні логування для різних середовищ
- Реалізуйте моніторинг продуктивності та здоров'я системи

```csharp
// Налаштування логування
builder.Host.UseSerilog((ctx, lc) => lc
    .WriteTo.Console()
    .WriteTo.File("logs/komunalka-api-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .ReadFrom.Configuration(ctx.Configuration));
```

## Регулярні практики

- Проводьте код-рев'ю для всіх змін
- Регулярно оновлюйте залежності
- Виконуйте статичний аналіз коду з використанням інструментів як SonarQube
- Проводьте рефакторинг коду для підтримки якості

## Висновок

Дотримання цих рекомендацій допоможе створювати якісний, підтримуваний та ефективний код в проекті Komunalka API. Пам'ятайте, що ці рекомендації можуть еволюціонувати з часом разом із розвитком проекту та технологій.
