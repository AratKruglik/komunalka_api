Komunalka API – Developer Guidelines (Project‑Specific)

Audience: Senior developers working on this repository. This document captures non-obvious, project-specific knowledge to speed up onboarding and reduce friction.

1) Build and Configuration
- SDK: .NET 9.0 is required. Verify with: dotnet --list-sdks (developed/tested with 9.0.101). 
- Project entry: KomunalkaAPI.csproj (ASP.NET Core Web API).
- Environment loading: Program.cs uses DotNetEnv.Env.Load(), so a .env file at repo root is honored in addition to appsettings*.json. The effective configuration builds as follows:
  - appsettings.json
  - appsettings.{ASPNETCORE_ENVIRONMENT}.json (optional)
  - Environment variables (including those loaded from .env)

Mandatory environment variables
The app fails fast at startup if JWT variables are missing; DB connectivity also requires variables.
- PostgreSQL (used by EF Core via Npgsql):
  - POSTGRES_HOST
  - POSTGRES_PORT
  - POSTGRES_DATABASE
  - POSTGRES_USERNAME
  - POSTGRES_PASSWORD
- JWT:
  - JWT_SECRET (>= 32 chars in production)
  - JWT_ISSUER
  - JWT_AUDIENCE
  - Optional but commonly used: JWT_EXPIRATION_MINUTES, JWT_REFRESH_TOKEN_EXPIRATION_DAYS (if referenced by services)
- Kestrel listen ports (optional overrides):
  - ASPNETCORE_HTTP_PORT (default 5095)
  - ASPNETCORE_HTTPS_PORT (default 7095)

DB connection string is composed from env in Program.cs. There is also a SqlServer package in the csproj, but the active provider is Npgsql; keep PostgreSQL in sync with migrations.

Running locally
- With Docker (preferred for DB):
  - docker-compose up -d (API exposed as documented in README.md: http://localhost:8080, https://localhost:8081)
- Without Docker:
  - Ensure PostgreSQL is running and credentials match .env.
  - Apply migrations: dotnet ef database update
  - Run API: dotnet run

Seeding
- On application start, Data/SeedData.SeedAsync is executed within a scoped service. It seeds Regions and AddressTypes if tables are empty. This affects local/dev environments. If you need a clean DB each run, clear respective tables before restart.

Swagger
- Swagger is enabled when ASPNETCORE_ENVIRONMENT=Development.

2) Testing
There is no test project in the repo by default. Below are two approaches, depending on environment constraints.

A) Standard unit testing with xUnit (recommended)
Prerequisites: NuGet access to restore packages.
- Create a test project (example):
  - dotnet new xunit -n KomunalkaAPI.Tests
  - cd KomunalkaAPI.Tests
  - dotnet add package FluentAssertions
  - dotnet add reference ../KomunalkaAPI.csproj
- Sample test:
  using Xunit;
  using FluentAssertions;
  
  public class MathSpecs
  {
      [Fact]
      public void Two_plus_two_is_four()
      {
          (2 + 2).Should().Be(4);
      }
  }
- Run tests:
  - From solution root or test project dir: dotnet test KomunalkaAPI.Tests/KomunalkaAPI.Tests.csproj
Notes:
- If you’re behind a firewall or CI has no NuGet access, restore will fail. Use the offline smoke check (B) below in that case.

B) Offline smoke check (works without NuGet)
If package restore is restricted, you can still validate the toolchain and basic assumptions without external packages by using a temporary console project that references the API project.
- Create a temporary console project (not committed):
  - dotnet new console -n KomunalkaAPI.SmokeCheck
  - dotnet add KomunalkaAPI.SmokeCheck/KomunalkaAPI.SmokeCheck.csproj reference KomunalkaAPI.csproj
- Replace Program.cs content with a basic assertion (no external packages):
  using System;
  
  public static class Program
  {
      public static int Main(string[] args)
      {
          if (2 + 2 != 4) return 1;
          Console.WriteLine("Smoke check OK");
          return 0;
      }
  }
- Run: dotnet run --project KomunalkaAPI.SmokeCheck/KomunalkaAPI.SmokeCheck.csproj
- Remove the temporary project after validation to keep repo clean.

Integration testing tips (when you add tests)
- Avoid hitting the real DB. Prefer an ephemeral Postgres instance (Testcontainers) or a dedicated test schema. Alternately, abstract data access behind repositories and mock those for pure unit tests.
- For API-layer tests, consider Microsoft.AspNetCore.Mvc.Testing and WebApplicationFactory to host the server in-memory. Configure the test host to override DB to an in-memory or containerized instance. Ensure SeedData is either disabled or controlled in tests to avoid nondeterminism.

3) Adding new tests to this repository
- Create a folder KomunalkaAPI.Tests at repo root.
- Use xUnit and add ProjectReference to KomunalkaAPI.csproj.
- Place test categories by technical layer (e.g., Services/, Repositories/, Controllers/). Example csproj snippet:
  <Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
      <TargetFramework>net9.0</TargetFramework>
      <IsPackable>false</IsPackable>
    </PropertyGroup>
    <ItemGroup>
      <PackageReference Include="xunit" Version="2.9.2" />
      <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      </PackageReference>
      <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
      <PackageReference Include="FluentAssertions" Version="6.12.0" />
    </ItemGroup>
    <ItemGroup>
      <ProjectReference Include="../KomunalkaAPI.csproj" />
    </ItemGroup>
  </Project>
- Running: dotnet test KomunalkaAPI.Tests/KomunalkaAPI.Tests.csproj

4) EF Core and Database
- Creating migrations: dotnet ef migrations add <Name>
- Applying migrations: dotnet ef database update
- Provider: Npgsql (PostgreSQL). Ensure your local DB matches the Npgsql version in csproj (Npgsql.EntityFrameworkCore.PostgreSQL 9.0.x).
- Common pitfalls:
  - Missing env vars: JWT_SECRET, JWT_ISSUER, JWT_AUDIENCE must be set, otherwise startup throws InvalidOperationException.
  - Connection string: All POSTGRES_* env vars must be present; otherwise startup fails when building DbContext.
  - Seeding runs on startup and assumes the DB is reachable; failed connection will fail the app.

5) Useful project-specific tips
- HTTP scratchpad: Komunalka.API.http contains sample requests (e.g., user creation with test emails). It’s useful for manual endpoint checks in Rider/VS Code.
- Ports: Program.cs adds http://localhost:{ASPNETCORE_HTTP_PORT} and https://localhost:{ASPNETCORE_HTTPS_PORT}. If you use Docker, docker-compose maps to 8080/8081; otherwise defaults are 5095/7095.
- Authentication: Swagger has a Bearer scheme configured. Obtain a JWT via the auth endpoints (see Controllers/ and Services/Auth) and use Authorize in Swagger UI in Development.
- Repository / UoW: IUnitOfWork is registered; prefer injecting repositories via DI instead of newing contexts. Keep transaction boundaries consistent.
- Serialization: System.Text.Json is configured to ignore cycles and skip nulls. Be mindful when adding navigation properties to DTOs.

6) Code style and conventions (delta vs common guidelines)
- DTOs: Keep them flat; avoid leaking EF entities to the API surface.
- Async: All data access should be async; suffix methods with Async consistently.
- Naming: PascalCase for public types/members; camelCase for locals/parameters.
- Validation: Add data annotations to Models and consider FluentValidation (if added later) for complex rules.

7) Troubleshooting
- Build succeeds but API fails immediately: check that JWT_* and POSTGRES_* env vars exist (dotenv loads .env at repo root).
- dotnet ef not found: dotnet tool install --global dotnet-ef
- Migrations provider mismatch: Ensure Npgsql is the active provider; do not create migrations while pointing at SqlServer packages.
- Port conflicts: Override ASPNETCORE_HTTP_PORT/HTTPS if the defaults are in use.

8) Housekeeping
- Do not commit .env with secrets. Use .env.example as a template.
- Keep docs/README.md and docs/GUIDELINES.md in sync when changing the development process.
- When adding tests, keep them in a separate test project; avoid test-only code in the main Web API project.

Verification note
- In this session, the project builds successfully with .NET 9. A temporary xUnit test project could not be executed in this environment due to NuGet restore restrictions. As an alternative, a no-NuGet smoke check approach (section 2B) was validated and is recommended for offline CI or restricted environments. Once NuGet is available, use the standard xUnit path (section 2A).
