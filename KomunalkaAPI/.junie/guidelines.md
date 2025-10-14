Komunalka API – Junie Context Guidelines (Concise)

Purpose
- This file is the primary context Junie uses during this repo’s sessions. Keep it short, accurate, and in sync with docs/.

Project summary
- UA: Це API для ведення статистики по комунальних платежах для однієї або багатьох адрес користувача. Мета — збирати та агрегувати показники з лічильників комунальних послуг: газ, водопостачання (холодне й гаряче), електроенергія (денний і нічний тарифи). Дані зберігаються по адресах, з підтримкою тарифів і історії показників для подальшої аналітики.
- EN: This API tracks utility payment statistics for a single or multiple user addresses. Its goal is to collect and aggregate readings from utility meters: gas, water supply (cold and hot), and electricity (day/night tariffs). Data is stored per address, with tariff support and historical meter readings for analytics.

1) Build & Configuration
- SDK: .NET 9.0 (verify: dotnet --list-sdks; developed with 9.0.101)
- Entry: KomunalkaAPI.csproj (ASP.NET Core Web API)
- Config loading order: appsettings.json → appsettings.{ENV}.json → environment variables (including .env via DotNetEnv.Env.Load())
- Mandatory env vars:
  - PostgreSQL: POSTGRES_HOST, POSTGRES_PORT, POSTGRES_DATABASE, POSTGRES_USERNAME, POSTGRES_PASSWORD
  - JWT: JWT_SECRET (>= 32 chars prod), JWT_ISSUER, JWT_AUDIENCE; optional: JWT_EXPIRATION_MINUTES, JWT_REFRESH_TOKEN_EXPIRATION_DAYS (if used)
  - Ports (optional overrides): ASPNETCORE_HTTP_PORT (5095), ASPNETCORE_HTTPS_PORT (7095)
- Connection string is composed in Program.cs from POSTGRES_*; EF provider: Npgsql
- Run locally:
  - Docker (preferred DB): docker-compose up -d (API: http://localhost:8080, https://localhost:8081)
  - Without Docker: ensure PostgreSQL is up, dotnet ef database update, then dotnet run
- Swagger is enabled for ASPNETCORE_ENVIRONMENT=Development

2) Architecture (see docs/ARCHITECTURE.md)
- Layers:
  - Presentation: Controllers/, DTO/
  - Business: Services/, Models/
  - Data Access: Repositories/, Data/
- Patterns: Repository + Unit of Work for data abstraction and transactions
- Data flow: DTO → Controller → Service/UoW → Repository → DB → DTO
- Core entities: User, Address, ServiceCategory, ServiceCounter, ServiceCounterValue, Tariff

3) Authentication (see docs/AUTHENTICATION.md)
- JWT Bearer; refresh tokens are single-use, stored in DB, can be revoked
- Components: JwtService (token gen/validation), AuthService (business logic), AuthController (endpoints)
- Key endpoints: POST /api/auth/register, /login, /refresh-token, /revoke-token; GET /validate-token
- Protect endpoints with [Authorize]; roles via [Authorize(Roles = "...")]

4) Seeding
- Data/SeedData.SeedAsync runs on startup; seeds Regions and AddressTypes if empty

5) Testing
- xUnit (recommended): separate project KomunalkaAPI.Tests with ProjectReference to KomunalkaAPI.csproj; typical packages: xunit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, FluentAssertions
- Offline smoke check (no NuGet): temporary console app referencing KomunalkaAPI.csproj; assert basics and run
- Integration tests: avoid real DB; use containerized Postgres or abstractions with mocks

6) EF Core & Database
- Provider: Npgsql (PostgreSQL); keep migrations aligned with provider versions
- Commands: dotnet ef migrations add <Name>; dotnet ef database update
- Pitfalls: missing JWT_* or POSTGRES_* causes startup failure; failed DB connection breaks seeding

7) Code conventions
- DTOs should be flat; do not leak EF entities through API
- Async for I/O; suffix Async consistently
- Naming: PascalCase (public types/members), camelCase (locals/parameters)
- Validation: DataAnnotations on models; consider FluentValidation for complex rules (if added)
- Serialization: System.Text.Json configured to ignore cycles and skip nulls (Program.cs)

8) Ports & integrations
- Defaults: 5095 HTTP / 7095 HTTPS; Docker maps to 8080/8081
- Swagger includes Bearer auth; obtain JWT via auth endpoints and authorize in Development

9) Troubleshooting
- API crashes on startup: check JWT_* and POSTGRES_* (including .env)
- dotnet ef missing: dotnet tool install --global dotnet-ef
- Migrations/provider mismatch: ensure Npgsql is active; do not create migrations targeting SqlServer
- Port conflicts: override ASPNETCORE_HTTP_PORT/HTTPS

10) Housekeeping
- Do not commit .env with secrets; use .env.example
- Keep docs/README.md, docs/GUIDELINES.md, and this file in sync when processes change
- Tests live in a separate test project; avoid test-only code in the Web API project

Note
- Junie should prioritize this file for quick context and use docs/ARCHITECTURE.md and docs/AUTHENTICATION.md for deeper details.