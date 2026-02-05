# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Komunalka API is an ASP.NET Core (.NET 10.0) API for managing utility services, bills, and meter readings. The project uses PostgreSQL with Entity Framework Core and implements JWT authentication.

## Tech Stack

- **Framework**: ASP.NET Core (.NET 10.0)
- **Language**: C# 13.0
- **Database**: PostgreSQL 17
- **ORM**: Entity Framework Core 10.0
- **Authentication**: JWT with refresh tokens
- **Password Hashing**: BCrypt.Net-Next
- **Environment Variables**: DotNetEnv
- **Containerization**: Docker

## Common Commands

### Running the Application

```bash
# With Docker (recommended)
docker-compose up -d

# Without Docker
dotnet run

# Development with auto-reload
dotnet watch run
```

API will be available at:
- HTTP: http://localhost:8080 (Docker) or http://localhost:5095 (local)
- HTTPS: https://localhost:8081 (Docker) or https://localhost:7095 (local)
- Swagger UI: Available in Development environment at /swagger

### Database Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName

# Apply migrations to database
dotnet ef database update

# Rollback to specific migration
dotnet ef database update MigrationName

# Remove last migration (if not applied)
dotnet ef migrations remove
```

### Build and Test

```bash
# Build the project
dotnet build

# Build in Release mode
dotnet build -c Release

# Clean build artifacts
dotnet clean
```

### Docker Commands

```bash
# Start services
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down

# Rebuild and restart
docker-compose up -d --build

# Access pgAdmin
# Navigate to http://localhost:5050
# Login: admin@admin.com / admin
```

## Architecture

### Layered Architecture

The project follows a layered architecture with clear separation of concerns:

**Presentation Layer:**
- `Controllers/` - REST API endpoints
- `DTO/` - Data Transfer Objects for API communication

**Business Logic Layer:**
- `Services/` - Business logic implementation (e.g., JwtService, AuthService)
- `Models/` - Domain entities

**Data Access Layer:**
- `Repositories/` - Repository pattern implementations
- `Data/` - DbContext, migrations, and seed data

### Repository and Unit of Work Pattern

The project implements Repository pattern with Unit of Work for data access:

- **Generic Repository**: Base `Repository<T>` provides common CRUD operations
- **Specific Repositories**: Interface and implementation per entity (e.g., `IUserRepository`, `UserRepository`)
- **Unit of Work**: `IUnitOfWork` coordinates repositories and manages transactions via `CompleteAsync()`

Key pattern: Controllers use `IUnitOfWork` to access repositories, modify entities, then call `CompleteAsync()` to persist all changes in a single transaction.

Example flow:
```csharp
// In controller
var user = await _unitOfWork.Users.GetByEmailAsync(email);
user.UpdateProperty();
await _unitOfWork.CompleteAsync(); // Saves all changes
```

### Authentication Architecture

JWT authentication with refresh token mechanism:

- **JwtService** (`Services/Auth/JwtService.cs`): Generates and validates JWT tokens and refresh tokens
- **AuthService** (`Services/Auth/AuthService.cs`): Implements registration, login, token refresh, and revocation logic
- **AuthController** (`Controllers/AuthController.cs`): Exposes authentication endpoints

Authentication flow:
1. User registers/logs in → receives JWT + refresh token
2. JWT used for API authorization (short-lived, 30 min default)
3. Refresh token used to obtain new JWT (long-lived, 7 days default)
4. Refresh tokens stored in database and can be revoked

### Configuration and Environment

Configuration is loaded from multiple sources (order matters):
1. `appsettings.json` - Base configuration
2. `appsettings.{Environment}.json` - Environment-specific config
3. `.env` file - Sensitive data (database credentials, JWT secrets)
4. Environment variables - Override everything

Required environment variables in `.env`:
- Database: `POSTGRES_HOST`, `POSTGRES_PORT`, `POSTGRES_DATABASE`, `POSTGRES_USERNAME`, `POSTGRES_PASSWORD`
- JWT: `JWT_SECRET`, `JWT_ISSUER`, `JWT_AUDIENCE`, `JWT_EXPIRATION_MINUTES`, `JWT_REFRESH_TOKEN_EXPIRATION_DAYS`
- Ports (optional): `ASPNETCORE_HTTP_PORT`, `ASPNETCORE_HTTPS_PORT`

### Database Structure

Main entities:
- `User` - Users with JWT-based authentication
- `RefreshToken` - Refresh tokens for JWT renewal
- `Address` - Property addresses
- `Region` - Geographic regions
- `AddressType` - Types of addresses (apartment, house, etc.)
- `ServiceCategory` - Utility service categories
- `ServiceCounter` - Utility meters
- `ServiceCounterValue` - Meter readings
- `ServiceCounterMeasurement` - Measurement units
- `Tariff` - Service tariffs
- `Currency` - Currencies for tariffs
- `AddressesServiceCategory` - Many-to-many relationship between addresses and services

Seed data is automatically applied on application startup via `SeedData.SeedAsync()` in `Program.cs`.

## Development Workflow

### Adding a New Entity

1. Create model in `Models/` with data annotations
2. Add `DbSet<Entity>` to `ApplicationDbContext.cs`
3. Create migration: `dotnet ef migrations add AddEntity`
4. Create DTO in `DTO/` (typically `EntityDto`, `CreateEntityDto`, `UpdateEntityDto`)
5. Create repository interface in `Repositories/Entity/IEntityRepository.cs` extending `IRepository<Entity>`
6. Create repository implementation in `Repositories/Entity/EntityRepository.cs` extending `Repository<Entity>`
7. Add repository property to `IUnitOfWork` and `UnitOfWork`
8. Register repository in `Program.cs` if needed (most are created via UnitOfWork)
9. Create controller in `Controllers/` using the repository via UnitOfWork
10. Apply migration: `dotnet ef database update`

### Protecting Endpoints

Use `[Authorize]` attribute for JWT-protected endpoints:
```csharp
[Authorize] // Requires valid JWT
[HttpGet("protected")]
public IActionResult Protected() { ... }

[Authorize(Roles = "Admin")] // Requires specific role
[HttpGet("admin-only")]
public IActionResult AdminOnly() { ... }
```

Access current user claims in controllers:
```csharp
var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var email = User.FindFirst(ClaimTypes.Email)?.Value;
var role = User.FindFirst(ClaimTypes.Role)?.Value;
```

### JSON Serialization Settings

Configured in `Program.cs`:
- `ReferenceHandler.IgnoreCycles` - Prevents circular reference issues
- `WriteIndented = true` - Pretty-printed JSON
- `DefaultIgnoreCondition.WhenWritingNull` - Omits null values

## Coding Standards

From `docs/CONTRIBUTING.md`:

- **Naming**: camelCase for local variables, PascalCase for classes/methods/properties
- **Async**: Always use async methods for database operations
- **Comments**: Code should be self-descriptive. Avoid comments that explain "what" the code does — use meaningful names instead. Comments are acceptable only for:
  - Non-obvious "why" decisions (business logic reasoning, workarounds)
  - Public API documentation (XML docs for libraries/SDKs)
  - Complex algorithms that can't be simplified
  - TODO/FIXME markers for technical debt
- **Commit messages**: Format as `type: message` where type is `feat`, `fix`, `docs`, `style`, `refactor`, `test`, or `chore`

Example:
```
feat: додано функціонал автентифікації
fix: виправлено помилку в розрахунку тарифів
docs: оновлено документацію API
```

## Documentation

Additional documentation in `docs/`:
- `ARCHITECTURE.md` - Detailed architecture documentation
- `AUTHENTICATION.md` - Complete authentication guide with endpoints and examples
- `CONTRIBUTING.md` - Contribution guidelines
