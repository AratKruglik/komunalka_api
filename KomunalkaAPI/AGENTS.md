# Repository Guidelines

## Project Structure & Module Organization
Treat `docs/` (esp. `GUIDELINES.md`, `ARCHITECTURE.md`, `AUTHENTICATION.md`) as the primary reference before changing code. `Controllers/` should stay thin, translating HTTP to calls into `Services/`. `Services/` and `Repositories/` implement the business and data layers described in docs, while `Data/` wraps `ApplicationDbContext` and Unit-of-Work wiring; schema state lives in `Migrations/`. Domain contracts sit in `Models/` and `DTO/`, validators in `Validators/`, AutoMapper profiles in `Mapping/`, and cross-cutting glue in `Middleware/` and `Extensions/`. `Dockerfile`/`compose.yml` provision the API with Postgres; `Komunalka.API.http` contains sample requests for smoke checks.

## Build, Test, and Development Commands
Restore packages with `dotnet restore` and build via `dotnet build`. Load configuration from `.env` (copied from `.env.example`) before running `dotnet ef database update` to apply migrations through Npgsql. Use `dotnet watch run --launch-profile https` for hot reload on `https://localhost:7149` / `http://localhost:5242`, or `docker compose up --build` to start the API plus Postgres on `https://localhost:8081` / `http://localhost:8080`. Swagger is available in Development; verify `/swagger` renders after your change.

## Coding Style & Naming Conventions
Follow the four-space, nullable-enabled defaults noted in `docs/GUIDELINES.md`. Keep async flows async end-to-end with `Async` suffixes, return DTOs rather than EF entities, and centralize dependency registration inside `Extensions/`. Use PascalCase for types and public members, camelCase for locals and injected services. AutoMapper profiles should map DTOs explicitly to avoid silent property gaps, and controllers should defer validation to FluentValidation or data annotations in `Validators/`.

## Testing Guidelines
Align tests with guidance in `docs/GUIDELINES.md`: organize an xUnit test project (e.g., `tests/KomunalkaAPI.Tests`) mirroring the namespace layout of `Controllers/` and `Services/`. Name classes `{Feature}Tests` and methods `MethodName_ShouldExpectedOutcome`. Prefer the EF Core InMemory provider or disposable Postgres containers for integration scenarios. Run `dotnet test` before pushing and ensure new migrations compile.

## Commit & Pull Request Guidelines
Continue the Conventional Commit convention from Git history (`feat:`, `fix:`, `chore:`). Reference issues or user stories, keep subject lines ≤72 characters, and describe schema/config updates plus testing steps (`dotnet test`, `docker compose up`). PRs should call out changes to auth flow or seed data sourced from docs, and include JSON samples or Swagger screenshots when behavior shifts.

## Security & Configuration Tips
Per `docs/GUIDELINES.md`, never commit real secrets. Duplicate `.env.example`, generate ≥32-character `JWT_SECRET`, and rely on environment overrides instead of editing `appsettings.json` for production settings. Scrub `logs/` before sharing diagnostics, and rotate refresh tokens through the Auth service when revoking access.
