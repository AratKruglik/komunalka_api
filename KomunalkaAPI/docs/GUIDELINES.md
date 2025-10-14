# Керівництво розробника Komunalka API (коротка версія)

Це стисле керівництво узгоджене з ARCHITECTURE.md і AUTHENTICATION.md. Тримайте його в фокусі під час розробки.

Опис проєкту / Project summary
- UA: Це API для ведення статистики по комунальних платежах для однієї або багатьох адрес користувача. Мета — збирати та агрегувати показники з лічильників комунальних послуг: газ, водопостачання (холодне й гаряче), електроенергія (денний і нічний тарифи). Дані зберігаються по адресах, з підтримкою тарифів і історії показників для подальшої аналітики.
- EN: This API tracks utility payment statistics for a single or multiple user addresses. Its goal is to collect and aggregate readings from utility meters: gas, water supply (cold and hot), and electricity (day/night tariffs). Data is stored per address, with tariff support and historical meter readings for analytics.

1) Збірка та конфігурація
- SDK: .NET 9.0 (перевірка: `dotnet --list-sdks`). Проєкт: KomunalkaAPI.csproj (ASP.NET Core Web API).
- Завантаження конфігурації: appsettings.json → appsettings.{ENV}.json → змінні середовища (включно з .env через DotNetEnv.Env.Load()).
- Обов’язкові змінні середовища:
  - PostgreSQL: POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DATABASE, POSTGRES_USERNAME, POSTGRES_PASSWORD
  - JWT: JWT_SECRET (>= 32 символи прод), JWT_ISSUER, JWT_AUDIENCE; додатково: JWT_EXPIRATION_MINUTES, JWT_REFRESH_TOKEN_EXPIRATION_DAYS (за наявності в коді)
  - Порти Kestrel (за потреби): ASPNETCORE_HTTP_PORT (5095), ASPNETCORE_HTTPS_PORT (7095)
- Рядок підключення формується в Program.cs із змінних POSTGRES_*. Активний провайдер EF: Npgsql.
- Запуск:
  - Docker (рекомендовано для БД): `docker-compose up -d` (API: http://localhost:8080, https://localhost:8081)
  - Локально: запустіть PostgreSQL, застосуйте міграції `dotnet ef database update`, далі `dotnet run`.
- Swagger: вмикається для ASPNETCORE_ENVIRONMENT=Development.

2) Архітектура (див. docs/ARCHITECTURE.md)
- Шари:
  - Presentation: Controllers/, DTO/
  - Business: Services/, Models/
  - Data Access: Repositories/, Data/
- Патерни: Repository та Unit of Work для абстракції доступу до даних і транзакційності.
- Потік даних: DTO → контролер → сервіс/UnitOfWork → репозиторій → БД → назад у DTO.
- Сутності (основні): User, Address, ServiceCategory, ServiceCounter, ServiceCounterValue, Tariff.

3) Аутентифікація (див. docs/AUTHENTICATION.md)
- JWT Bearer; refresh-токени одноразові, зберігаються в БД, можуть бути відкликані.
- Компоненти: JwtService (генерація/перевірка), AuthService (бізнес-логіка), AuthController (ендпоінти).
- Базові ендпоінти: POST /api/auth/register, /login, /refresh-token, /revoke-token; GET /validate-token.
- Захист ендпоінтів: [Authorize], роли через [Authorize(Roles = "...")].

4) Посів (Seeding)
- Data/SeedData.SeedAsync виконується на старті (Regions, AddressTypes якщо порожньо). Для чистої БД — очищайте відповідні таблиці перед рестартом.

5) Тестування
- xUnit (рекомендовано): окремий проєкт KomunalkaAPI.Tests з посиланням на KomunalkaAPI.csproj; приклади пакетів у docs/README.md та розділі нижче.
- Офлайн smoke check (без NuGet): тимчасовий консольний проєкт, який посилається на KomunalkaAPI.csproj; перевіряє базову працездатність (див. вказівки у проектних нотатках).
- Інтеграційні тести: не бийтеся в реальну БД; використовуйте контейнерну/тимчасову БД або абстракції з моками.

6) EF Core і БД
- Провайдер: Npgsql (PostgreSQL). Синхронізуйте версії пакунків та міграцій.
- Команди: `dotnet ef migrations add <Name>`; `dotnet ef database update`.
- Типові помилки: відсутні JWT_* або POSTGRES_* → падіння на старті; невдала БД-підключення зламає посів.

7) Конвенції коду
- DTO — пласкі; не витікаємо EF сутностями в API.
- Async скрізь для I/O; суфікс Async обов’язковий.
- Найменування: PascalCase для публічних типів/членів; camelCase — для локальних/параметрів.
- Валідація: data annotations у моделях; для складного — FluentValidation (за потреби).
- Серіалізація: System.Text.Json — ігнор циклів, пропуск null (конфіг у Program.cs).

8) Порти та інтеграції
- Локальні порти за замовчуванням: 5095/7095; у Docker — 8080/8081.
- Swagger має схему Bearer; отримайте JWT через ендпоінти автентифікації та авторизуйтесь у Swagger UI (Development).

9) Troubleshooting
- API одразу падає: перевірте JWT_* і POSTGRES_* (включно з .env).
- `dotnet ef` відсутній: встановіть інструмент (`dotnet tool install --global dotnet-ef`).
- Міграції vs провайдер: переконайтеся, що активний Npgsql; не створюйте міграції під SqlServer.
- Конфлікт портів: перевизначте ASPNETCORE_HTTP_PORT/HTTPS.

10) Housekeeping
- Не комітьте .env із секретами; використовуйте .env.example.
- Оновлюйте docs/README.md і це керівництво при зміні процесів.
- Тести — у окремому тест-проєкті; не додавайте тестовий код у Web API.

Джерела: деталі архітектури — docs/ARCHITECTURE.md; автентифікації — docs/AUTHENTICATION.md. Залишайте це керівництво коротким і актуальним.
