# OAuth 2.0 Implementation Plan
## Social Media Authentication (Google, Apple, GitHub)

**Створено:** 2025-12-12
**Статус:** Planning
**Бранч:** `feature/social-media-login`

---

## 📋 Зміст

1. [Огляд архітектури](#1-огляд-архітектури)
2. [Зміни в моделях даних](#2-зміни-в-моделях-даних)
3. [DTO та контракти](#3-dto-та-контракти)
4. [Сервіси та бізнес-логіка](#4-сервіси-та-бізнес-логіка)
5. [Контролери та endpoints](#5-контролери-та-endpoints)
6. [Налаштування провайдерів](#6-налаштування-провайдерів)
7. [Міграції бази даних](#7-міграції-бази-даних)
8. [Безпека](#8-безпека)
9. [Тестування](#9-тестування)
10. [Deployment та конфігурація](#10-deployment-та-конфігурація)

---

## 1. Огляд архітектури

### 1.1 Поточна система автентифікації

```
User Registration/Login (Email + Password)
    ↓
AuthService → BCrypt hashing
    ↓
JwtService → Generate JWT + RefreshToken
    ↓
Store RefreshToken in DB
    ↓
Return AuthenticationResponse
```

### 1.2 Нова система з OAuth 2.0

```
┌─────────────────────────────────────────────────────────────┐
│                    Authentication Flow                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Traditional                        OAuth 2.0                │
│  ┌──────────┐                      ┌──────────┐             │
│  │  Email   │                      │  Google  │             │
│  │   +      │                      │  Apple   │             │
│  │ Password │                      │  GitHub  │             │
│  └────┬─────┘                      └────┬─────┘             │
│       │                                 │                   │
│       ├─────────────┬───────────────────┤                   │
│       │             │                   │                   │
│       ▼             ▼                   ▼                   │
│  AuthService   OAuthService      ExternalProvider          │
│       │             │                   │                   │
│       └─────────────┴───────────────────┘                   │
│                     │                                       │
│                     ▼                                       │
│            UserManagementService                            │
│            (Find or Create User)                            │
│                     │                                       │
│                     ▼                                       │
│               JwtService                                    │
│            (Generate Tokens)                                │
│                     │                                       │
│                     ▼                                       │
│          AuthenticationResponse                             │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 1.3 Ключові принципи дизайну

1. **Single Responsibility**: Кожен сервіс має одну чітку відповідальність
2. **Open/Closed**: Легко додавати нові провайдери без зміни існуючого коду
3. **Dependency Inversion**: Залежність на абстракції, а не конкретні реалізації
4. **Low Cognitive Complexity**: Простий та зрозумілий код

---

## 2. Зміни в моделях даних

### 2.1 Модель `User`

**Файл:** `Models/User.cs`

#### Зміни:

1. **Password** - зробити nullable (OAuth користувачі не мають паролю)
2. Додати нові поля для OAuth:

```csharp
// OAuth fields
[StringLength(50)]
public string? AuthProvider { get; set; } // "Local", "Google", "Apple", "GitHub"

[StringLength(500)]
public string? ExternalId { get; set; } // ID від провайдера

public bool EmailVerified { get; set; } = false; // OAuth email завжди verified

public DateTime? LastLoginAt { get; set; }
```

#### Обґрунтування:
- **AuthProvider**: Enum-подібне поле для ідентифікації джерела реєстрації
- **ExternalId**: Унікальний ID від OAuth провайдера (sub claim)
- **EmailVerified**: Важливо для безпеки, OAuth провайдери гарантують перевірений email
- **LastLoginAt**: Корисно для аналітики та безпеки

#### Індекси:
```csharp
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(AuthProvider), nameof(ExternalId), IsUnique = true, Name = "IX_User_Provider_ExternalId")]
```

### 2.2 Нова модель `UserExternalAuth` (Опціонально)

**Альтернативний підхід**: Якщо користувач може мати декілька OAuth провайдерів

**Файл:** `Models/UserExternalAuth.cs`

```csharp
public class UserExternalAuth
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public required string Provider { get; set; } // "Google", "Apple", "GitHub"

    [Required]
    [StringLength(500)]
    public required string ExternalId { get; set; } // sub from provider

    [StringLength(255)]
    public string? Email { get; set; } // Email from provider

    public string? AccessToken { get; set; } // Encrypted, для майбутніх інтеграцій

    public string? RefreshToken { get; set; } // Encrypted

    public DateTime? TokenExpiresAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey("User")]
    public int UserId { get; set; }

    public User? User { get; set; }
}
```

#### Переваги окремої таблиці:
- ✅ Користувач може мати декілька OAuth провайдерів
- ✅ Можна зберігати провайдер-специфічні дані
- ✅ Легше управляти зв'язками між провайдерами
- ✅ Краща нормалізація

#### Недоліки:
- ❌ Додаткова складність
- ❌ Додатковий JOIN при запитах
- ❌ Більше коду

**Рекомендація**: Почати з простого підходу (поля в User), перейти на UserExternalAuth якщо потрібно.

---

## 3. DTO та контракти

### 3.1 Request DTO

**Файл:** `DTO/Auth/OAuthLoginRequest.cs`

```csharp
public class OAuthLoginRequest
{
    [Required(ErrorMessage = "Provider is required")]
    [AllowedValues("Google", "Apple", "GitHub")]
    public required string Provider { get; set; }

    [Required(ErrorMessage = "ID token is required")]
    public required string IdToken { get; set; } // або AccessToken залежно від flow
}
```

**Файл:** `DTO/Auth/OAuthCallbackRequest.cs`

```csharp
public class OAuthCallbackRequest
{
    [Required]
    public required string Provider { get; set; }

    public string? Code { get; set; } // Authorization code

    public string? State { get; set; } // CSRF protection

    public string? Error { get; set; }

    public string? ErrorDescription { get; set; }
}
```

### 3.2 Internal DTO

**Файл:** `DTO/Auth/OAuthUserInfo.cs`

```csharp
/// <summary>
/// Normalized user info from OAuth provider
/// </summary>
public class OAuthUserInfo
{
    public required string Provider { get; set; }

    public required string ExternalId { get; set; } // sub claim

    public required string Email { get; set; }

    public bool EmailVerified { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Username { get; set; }

    public string? AvatarUrl { get; set; }

    public Dictionary<string, object>? AdditionalData { get; set; }
}
```

### 3.3 Зміни в існуючих DTO

**Файл:** `DTO/Auth/RegisterUserRequest.cs`

```csharp
// Password стає опціональним для OAuth
[StringLength(100, MinimumLength = 6)]
public string? Password { get; set; } // Required для local, optional для OAuth
```

**Файл:** `DTO/Auth/AuthenticationResponse.cs`

```csharp
// Додати інформацію про провайдера
public string? AuthProvider { get; set; }
public bool EmailVerified { get; set; }
```

---

## 4. Сервіси та бізнес-логіка

### 4.1 Структура сервісів

```
Services/
├── Auth/
│   ├── IAuthService.cs (existing)
│   ├── AuthService.cs (existing, потребує змін)
│   ├── IJwtService.cs (existing)
│   ├── JwtService.cs (existing)
│   ├── IOAuthService.cs (new)
│   ├── OAuthService.cs (new)
│   └── Providers/
│       ├── IOAuthProvider.cs (new - interface)
│       ├── GoogleOAuthProvider.cs (new)
│       ├── AppleOAuthProvider.cs (new)
│       └── GitHubOAuthProvider.cs (new)
```

### 4.2 IOAuthProvider Interface

**Файл:** `Services/Auth/Providers/IOAuthProvider.cs`

```csharp
public interface IOAuthProvider
{
    /// <summary>
    /// Provider name (Google, Apple, GitHub)
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Validate ID token and extract user info
    /// </summary>
    Task<OAuthUserInfo?> ValidateTokenAsync(string idToken);

    /// <summary>
    /// Exchange authorization code for tokens
    /// </summary>
    Task<OAuthUserInfo?> ExchangeCodeAsync(string code, string? redirectUri = null);

    /// <summary>
    /// Get authorization URL for OAuth flow
    /// </summary>
    string GetAuthorizationUrl(string state, string? redirectUri = null);
}
```

**Чому це добре:**
- ✅ Стратегія pattern - легко додавати нові провайдери
- ✅ Кожен провайдер інкапсулює свою логіку
- ✅ Тестування кожного провайдера незалежно
- ✅ Низька когнітивна складність

### 4.3 GoogleOAuthProvider

**Файл:** `Services/Auth/Providers/GoogleOAuthProvider.cs`

```csharp
public class GoogleOAuthProvider : IOAuthProvider
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GoogleOAuthProvider> _logger;

    public string ProviderName => "Google";

    public async Task<OAuthUserInfo?> ValidateTokenAsync(string idToken)
    {
        // 1. Verify ID token signature (using Google's public keys)
        // 2. Validate claims (iss, aud, exp)
        // 3. Extract user info
        // 4. Return normalized OAuthUserInfo
    }

    public async Task<OAuthUserInfo?> ExchangeCodeAsync(string code, string? redirectUri)
    {
        // 1. Exchange code for access_token and id_token
        // 2. Validate id_token
        // 3. Return user info
    }

    public string GetAuthorizationUrl(string state, string? redirectUri)
    {
        // Generate Google OAuth URL with proper scopes
    }
}
```

**Використання бібліотек:**
- `Google.Apis.Auth` для валідації токенів
- HttpClient для API requests

### 4.4 AppleOAuthProvider

**Особливості Apple Sign In:**
- Використовує JWT ES256 (не RS256 як Google)
- Потрібен Apple Developer account
- Private Key для створення client_secret
- User info приходить тільки при першому sign in
- Email може бути прихованим (private relay)

**Файл:** `Services/Auth/Providers/AppleOAuthProvider.cs`

```csharp
public class AppleOAuthProvider : IOAuthProvider
{
    public string ProviderName => "Apple";

    public async Task<OAuthUserInfo?> ValidateTokenAsync(string idToken)
    {
        // 1. Get Apple's public keys from https://appleid.apple.com/auth/keys
        // 2. Validate JWT signature using ES256
        // 3. Validate claims
        // 4. Extract user info (обмежена інформація)
    }

    // Apple specific: Generate client_secret
    private string GenerateClientSecret()
    {
        // Use Apple's Team ID, Key ID, and Private Key
        // Generate JWT signed with ES256
    }
}
```

**Використання бібліотек:**
- `System.IdentityModel.Tokens.Jwt` для валідації
- `Microsoft.IdentityModel.Tokens` для криптографії

### 4.5 GitHubOAuthProvider

**Особливості GitHub OAuth:**
- Не використовує ID tokens (OAuth 2.0, не OIDC)
- Потрібен окремий API request для user info
- Email може бути приватним
- Потрібен scope 'user:email' для email

**Файл:** `Services/Auth/Providers/GitHubOAuthProvider.cs`

```csharp
public class GitHubOAuthProvider : IOAuthProvider
{
    public string ProviderName => "GitHub";

    public async Task<OAuthUserInfo?> ValidateTokenAsync(string accessToken)
    {
        // GitHub не має ID tokens, використовує access token
        // 1. Call GitHub API /user with access token
        // 2. Call GitHub API /user/emails for verified email
        // 3. Return user info
    }

    public async Task<OAuthUserInfo?> ExchangeCodeAsync(string code, string? redirectUri)
    {
        // 1. Exchange code for access_token
        // 2. Get user info using access token
    }
}
```

### 4.6 IOAuthService

**Файл:** `Services/Auth/IOAuthService.cs`

```csharp
public interface IOAuthService
{
    /// <summary>
    /// Authenticate user via OAuth provider
    /// </summary>
    Task<AuthenticationResponse?> AuthenticateWithProviderAsync(string provider, string token);

    /// <summary>
    /// Handle OAuth callback (authorization code flow)
    /// </summary>
    Task<AuthenticationResponse?> HandleCallbackAsync(OAuthCallbackRequest request);

    /// <summary>
    /// Get authorization URL for provider
    /// </summary>
    string GetAuthorizationUrl(string provider, string? redirectUri = null);

    /// <summary>
    /// Link OAuth provider to existing user
    /// </summary>
    Task<bool> LinkProviderAsync(int userId, string provider, string token);

    /// <summary>
    /// Unlink OAuth provider from user
    /// </summary>
    Task<bool> UnlinkProviderAsync(int userId, string provider);
}
```

### 4.7 OAuthService Implementation

**Файл:** `Services/Auth/OAuthService.cs`

```csharp
public class OAuthService : IOAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IEnumerable<IOAuthProvider> _oauthProviders;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IEnumerable<IOAuthProvider> oauthProviders, // DI will inject all registered providers
        ILogger<OAuthService> logger)
    {
        // ...
    }

    public async Task<AuthenticationResponse?> AuthenticateWithProviderAsync(
        string providerName,
        string token)
    {
        // 1. Find appropriate provider
        var provider = GetProvider(providerName);
        if (provider == null)
            return null;

        // 2. Validate token and get user info
        var userInfo = await provider.ValidateTokenAsync(token);
        if (userInfo == null)
            return null;

        // 3. Find or create user
        var user = await FindOrCreateUserAsync(userInfo);
        if (user == null)
            return null;

        // 4. Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync();

        // 5. Generate JWT and refresh token
        return await GenerateAuthenticationResponseAsync(user);
    }

    private async Task<User?> FindOrCreateUserAsync(OAuthUserInfo userInfo)
    {
        // 1. Try to find by provider + externalId
        var user = await _unitOfWork.Users.GetByProviderAsync(
            userInfo.Provider,
            userInfo.ExternalId);

        if (user != null)
            return user;

        // 2. Try to find by email (user might have local account)
        user = await _unitOfWork.Users.GetByEmailAsync(userInfo.Email);

        if (user != null)
        {
            // Link OAuth to existing account
            user.AuthProvider = userInfo.Provider;
            user.ExternalId = userInfo.ExternalId;
            user.EmailVerified = userInfo.EmailVerified;
            await _unitOfWork.CompleteAsync();
            return user;
        }

        // 3. Create new user
        var newUser = new User
        {
            Username = GenerateUniqueUsername(userInfo),
            Email = userInfo.Email,
            FirstName = userInfo.FirstName,
            LastName = userInfo.LastName,
            Password = null, // OAuth user doesn't have password
            AuthProvider = userInfo.Provider,
            ExternalId = userInfo.ExternalId,
            EmailVerified = userInfo.EmailVerified,
            Role = "User"
        };

        await _unitOfWork.Users.AddAsync(newUser);
        await _unitOfWork.CompleteAsync();

        return newUser;
    }

    private string GenerateUniqueUsername(OAuthUserInfo userInfo)
    {
        // Generate username from email or name
        // Ensure uniqueness by checking DB and appending number if needed
        // Example: john.doe, john.doe2, john.doe3
    }

    private IOAuthProvider? GetProvider(string providerName)
    {
        return _oauthProviders.FirstOrDefault(
            p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }
}
```

**Чому це добре:**
- ✅ Strategy pattern для провайдерів
- ✅ Find-or-create логіка в одному місці
- ✅ Автоматичне зв'язування OAuth з існуючими акаунтами
- ✅ Легко тестувати (mock IOAuthProvider)

### 4.8 Зміни в AuthService

**Файл:** `Services/Auth/AuthService.cs`

**Зміни:**

1. Зробити Password опціональним при автентифікації:

```csharp
public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
{
    var user = await unitOfWork.Users.GetByEmailAsync(request.Email);

    if (user == null)
        return null;

    // OAuth users can't login with password
    if (user.Password == null)
    {
        logger.LogWarning("OAuth user attempted password login: {Email}", request.Email);
        return null;
    }

    if (!VerifyPassword(request.Password, user.Password))
        return null;

    // Update last login
    user.LastLoginAt = DateTime.UtcNow;
    await unitOfWork.CompleteAsync();

    return await GenerateAuthenticationResponseAsync(user);
}
```

2. Оновити RegisterAsync для підтримки AuthProvider:

```csharp
public async Task<AuthenticationResponse?> RegisterAsync(RegisterUserRequest request)
{
    // ... existing validation

    var newUser = new User
    {
        Username = request.Username,
        Email = request.Email,
        Password = HashPassword(request.Password),
        AuthProvider = "Local", // Mark as local registration
        EmailVerified = false, // Потрібна email verification
        Role = "User"
    };

    // ...
}
```

---

## 5. Контролери та endpoints

### 5.1 Нові endpoints в AuthController

**Файл:** `Controllers/AuthController.cs`

```csharp
[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IOAuthService _oauthService; // Add this

    // ... existing methods

    /// <summary>
    /// Login with OAuth provider (ID token flow)
    /// </summary>
    [HttpPost("oauth/login")]
    public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _oauthService.AuthenticateWithProviderAsync(
            request.Provider,
            request.IdToken);

        if (result == null)
            return BadRequest(new { Message = "OAuth authentication failed" });

        return Ok(result);
    }

    /// <summary>
    /// Get OAuth authorization URL
    /// </summary>
    [HttpGet("oauth/{provider}/authorize")]
    public IActionResult GetAuthorizationUrl(string provider, [FromQuery] string? redirectUri)
    {
        try
        {
            var url = _oauthService.GetAuthorizationUrl(provider, redirectUri);
            return Ok(new { AuthorizationUrl = url });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// OAuth callback (authorization code flow)
    /// </summary>
    [HttpPost("oauth/callback")]
    public async Task<IActionResult> OAuthCallback([FromBody] OAuthCallbackRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!string.IsNullOrEmpty(request.Error))
        {
            return BadRequest(new
            {
                Error = request.Error,
                Description = request.ErrorDescription
            });
        }

        var result = await _oauthService.HandleCallbackAsync(request);

        if (result == null)
            return BadRequest(new { Message = "OAuth callback processing failed" });

        return Ok(result);
    }

    /// <summary>
    /// Link OAuth provider to current user
    /// </summary>
    [Authorize]
    [HttpPost("oauth/link")]
    public async Task<IActionResult> LinkOAuthProvider([FromBody] OAuthLoginRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _oauthService.LinkProviderAsync(
            userId,
            request.Provider,
            request.IdToken);

        if (!result)
            return BadRequest(new { Message = "Failed to link provider" });

        return Ok(new { Message = "Provider linked successfully" });
    }

    /// <summary>
    /// Unlink OAuth provider from current user
    /// </summary>
    [Authorize]
    [HttpDelete("oauth/unlink/{provider}")]
    public async Task<IActionResult> UnlinkOAuthProvider(string provider)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _oauthService.UnlinkProviderAsync(userId, provider);

        if (!result)
            return BadRequest(new { Message = "Failed to unlink provider" });

        return Ok(new { Message = "Provider unlinked successfully" });
    }
}
```

### 5.2 API Flow Diagrams

#### Flow 1: ID Token Flow (Рекомендовано для mobile/SPA)

```
Mobile App / SPA
    ↓
1. Authenticate with Google/Apple/GitHub SDK
    ↓
2. Receive ID token from provider
    ↓
3. POST /api/v1/auth/oauth/login
   Body: { provider: "Google", idToken: "..." }
    ↓
Backend validates token with provider
    ↓
Find or create user
    ↓
Generate JWT + RefreshToken
    ↓
4. Return AuthenticationResponse
```

#### Flow 2: Authorization Code Flow (Рекомендовано для web)

```
Web Browser
    ↓
1. GET /api/v1/auth/oauth/google/authorize?redirectUri=...
    ↓
2. Redirect to Google OAuth
    ↓
3. User authorizes app
    ↓
4. Google redirects back: /callback?code=...&state=...
    ↓
5. POST /api/v1/auth/oauth/callback
   Body: { provider: "Google", code: "...", state: "..." }
    ↓
Backend exchanges code for tokens
    ↓
Validate tokens
    ↓
Find or create user
    ↓
6. Return AuthenticationResponse
```

---

## 6. Налаштування провайдерів

### 6.1 Google OAuth Setup

**Кроки:**

1. Створити проект в [Google Cloud Console](https://console.cloud.google.com/)
2. Enable Google+ API
3. Create OAuth 2.0 credentials
4. Configure consent screen
5. Add authorized redirect URIs

**Environment variables (.env):**

```env
# Google OAuth
GOOGLE_CLIENT_ID=your-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=your-client-secret
GOOGLE_REDIRECT_URI=https://yourdomain.com/api/v1/auth/oauth/callback
```

**appsettings.json:**

```json
{
  "OAuth": {
    "Google": {
      "ClientId": "${GOOGLE_CLIENT_ID}",
      "ClientSecret": "${GOOGLE_CLIENT_SECRET}",
      "RedirectUri": "${GOOGLE_REDIRECT_URI}",
      "Scopes": ["openid", "profile", "email"]
    }
  }
}
```

**NuGet Package:**
```bash
dotnet add package Google.Apis.Auth
```

### 6.2 Apple Sign In Setup

**Кроки:**

1. Enroll in [Apple Developer Program](https://developer.apple.com/) ($99/year)
2. Create App ID with Sign In with Apple capability
3. Create Service ID for web authentication
4. Create Private Key for client secret
5. Configure Return URLs

**Environment variables (.env):**

```env
# Apple Sign In
APPLE_CLIENT_ID=com.yourdomain.app
APPLE_TEAM_ID=YOUR_TEAM_ID
APPLE_KEY_ID=YOUR_KEY_ID
APPLE_PRIVATE_KEY_PATH=/path/to/AuthKey_XXXXX.p8
APPLE_REDIRECT_URI=https://yourdomain.com/api/v1/auth/oauth/callback
```

**appsettings.json:**

```json
{
  "OAuth": {
    "Apple": {
      "ClientId": "${APPLE_CLIENT_ID}",
      "TeamId": "${APPLE_TEAM_ID}",
      "KeyId": "${APPLE_KEY_ID}",
      "PrivateKeyPath": "${APPLE_PRIVATE_KEY_PATH}",
      "RedirectUri": "${APPLE_REDIRECT_URI}",
      "Scopes": ["name", "email"]
    }
  }
}
```

**NuGet Package:**
```bash
dotnet add package AspNet.Security.OAuth.Apple
```

### 6.3 GitHub OAuth Setup

**Кроки:**

1. Go to GitHub Settings → Developer settings → OAuth Apps
2. Create new OAuth App
3. Set Authorization callback URL
4. Get Client ID and Client Secret

**Environment variables (.env):**

```env
# GitHub OAuth
GITHUB_CLIENT_ID=your_github_client_id
GITHUB_CLIENT_SECRET=your_github_client_secret
GITHUB_REDIRECT_URI=https://yourdomain.com/api/v1/auth/oauth/callback
```

**appsettings.json:**

```json
{
  "OAuth": {
    "GitHub": {
      "ClientId": "${GITHUB_CLIENT_ID}",
      "ClientSecret": "${GITHUB_CLIENT_SECRET}",
      "RedirectUri": "${GITHUB_REDIRECT_URI}",
      "Scopes": ["user:email"]
    }
  }
}
```

**NuGet Package:**
```bash
dotnet add package AspNet.Security.OAuth.GitHub
```

### 6.4 Program.cs Configuration

**Файл:** `Program.cs`

```csharp
// Add OAuth providers
builder.Services.AddHttpClient(); // For OAuth HTTP requests

// Register OAuth providers
builder.Services.AddTransient<IOAuthProvider, GoogleOAuthProvider>();
builder.Services.AddTransient<IOAuthProvider, AppleOAuthProvider>();
builder.Services.AddTransient<IOAuthProvider, GitHubOAuthProvider>();

// Register OAuth service
builder.Services.AddScoped<IOAuthService, OAuthService>();

// Optional: Add built-in OAuth authentication (for cookie-based auth)
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["OAuth:Google:ClientId"]!;
        options.ClientSecret = builder.Configuration["OAuth:Google:ClientSecret"]!;
    })
    .AddApple(options =>
    {
        options.ClientId = builder.Configuration["OAuth:Apple:ClientId"]!;
        options.TeamId = builder.Configuration["OAuth:Apple:TeamId"]!;
        options.KeyId = builder.Configuration["OAuth:Apple:KeyId"]!;
        options.UsePrivateKey(keyId =>
            builder.Environment.ContentRootFileProvider.GetFileInfo(
                builder.Configuration["OAuth:Apple:PrivateKeyPath"]!));
    })
    .AddGitHub(options =>
    {
        options.ClientId = builder.Configuration["OAuth:GitHub:ClientId"]!;
        options.ClientSecret = builder.Configuration["OAuth:GitHub:ClientSecret"]!;
        options.Scope.Add("user:email");
    });
```

---

## 7. Міграції бази даних

### 7.1 Migration: AddOAuthSupport

**Команда:**
```bash
dotnet ef migrations add AddOAuthSupport
```

**Зміни:**

```csharp
public partial class AddOAuthSupport : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Make Password nullable
        migrationBuilder.AlterColumn<string>(
            name: "Password",
            table: "Users",
            nullable: true,
            oldNullable: false);

        // 2. Add OAuth fields to Users
        migrationBuilder.AddColumn<string>(
            name: "AuthProvider",
            table: "Users",
            maxLength: 50,
            nullable: true,
            defaultValue: "Local");

        migrationBuilder.AddColumn<string>(
            name: "ExternalId",
            table: "Users",
            maxLength: 500,
            nullable: true);

        migrationBuilder.AddColumn<bool>(
            name: "EmailVerified",
            table: "Users",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<DateTime>(
            name: "LastLoginAt",
            table: "Users",
            nullable: true);

        // 3. Create unique index on AuthProvider + ExternalId
        migrationBuilder.CreateIndex(
            name: "IX_User_Provider_ExternalId",
            table: "Users",
            columns: new[] { "AuthProvider", "ExternalId" },
            unique: true,
            filter: "\"AuthProvider\" IS NOT NULL AND \"ExternalId\" IS NOT NULL");

        // 4. Update existing users to have AuthProvider = 'Local'
        migrationBuilder.Sql(
            @"UPDATE ""Users""
              SET ""AuthProvider"" = 'Local'
              WHERE ""AuthProvider"" IS NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_User_Provider_ExternalId",
            table: "Users");

        migrationBuilder.DropColumn(name: "AuthProvider", table: "Users");
        migrationBuilder.DropColumn(name: "ExternalId", table: "Users");
        migrationBuilder.DropColumn(name: "EmailVerified", table: "Users");
        migrationBuilder.DropColumn(name: "LastLoginAt", table: "Users");

        migrationBuilder.AlterColumn<string>(
            name: "Password",
            table: "Users",
            nullable: false);
    }
}
```

### 7.2 Migration: AddUserExternalAuth (Якщо використовується окрема таблиця)

```bash
dotnet ef migrations add AddUserExternalAuth
```

---

## 8. Безпека

### 8.1 Token Validation

**Критично важливі перевірки:**

1. **Signature Verification**
   - Використовувати публічні ключі провайдера
   - Кешувати публічні ключі (з TTL)
   - Оновлювати ключі періодично

2. **Claims Validation**
   ```csharp
   // Validate issuer
   if (token.Issuer != "https://accounts.google.com")
       throw new SecurityTokenException("Invalid issuer");

   // Validate audience (your client ID)
   if (token.Audience != yourClientId)
       throw new SecurityTokenException("Invalid audience");

   // Validate expiration
   if (token.ValidTo < DateTime.UtcNow)
       throw new SecurityTokenException("Token expired");

   // Validate issued at (not too old)
   if (token.IssuedAt < DateTime.UtcNow.AddMinutes(-5))
       throw new SecurityTokenException("Token too old");
   ```

3. **Nonce Validation** (for OIDC)
   - Store nonce in session/cache
   - Validate nonce in ID token matches

### 8.2 CSRF Protection

**State parameter для authorization code flow:**

```csharp
public string GetAuthorizationUrl(string provider, string? redirectUri)
{
    // Generate cryptographically secure random state
    var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

    // Store state in cache with expiration (5 minutes)
    _cache.Set($"oauth_state_{state}", new
    {
        Provider = provider,
        RedirectUri = redirectUri,
        CreatedAt = DateTime.UtcNow
    }, TimeSpan.FromMinutes(5));

    // Include state in authorization URL
    return $"{authUrl}?client_id={clientId}&state={state}&...";
}

public async Task ValidateStateAsync(string state)
{
    var storedState = _cache.Get($"oauth_state_{state}");
    if (storedState == null)
        throw new SecurityException("Invalid state parameter");

    _cache.Remove($"oauth_state_{state}"); // One-time use
}
```

### 8.3 Rate Limiting

**Захист від brute force та DDoS:**

```csharp
// In Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("oauth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 2;
    });
});

// In controller
[EnableRateLimiting("oauth")]
[HttpPost("oauth/login")]
public async Task<IActionResult> OAuthLogin(...)
```

### 8.4 Sensitive Data Protection

**Не зберігати OAuth access tokens в plain text:**

```csharp
// If storing provider tokens (for API access later)
public class TokenEncryptionService
{
    private readonly IDataProtector _protector;

    public TokenEncryptionService(IDataProtectionProvider provider)
    {
        _protector = provider.CreateProtector("OAuthTokens");
    }

    public string Encrypt(string token) => _protector.Protect(token);

    public string Decrypt(string encryptedToken) => _protector.Unprotect(encryptedToken);
}
```

### 8.5 Email Verification

**Для local accounts потрібна email verification, для OAuth - ні:**

```csharp
public async Task<AuthenticationResponse?> AuthenticateAsync(...)
{
    // ...

    // Check if email is verified (only for local accounts)
    if (user.AuthProvider == "Local" && !user.EmailVerified)
    {
        return BadRequest(new { Message = "Email not verified" });
    }

    // ...
}
```

### 8.6 Account Takeover Prevention

**Не дозволяти automatic linking якщо є password:**

```csharp
private async Task<User?> FindOrCreateUserAsync(OAuthUserInfo userInfo)
{
    // Try find by email
    var user = await _unitOfWork.Users.GetByEmailAsync(userInfo.Email);

    if (user != null)
    {
        // If user has password (local account), don't auto-link
        if (user.Password != null)
        {
            _logger.LogWarning(
                "OAuth login attempt for email {Email} that has password set. Manual linking required.",
                userInfo.Email);

            return null; // Require manual linking
        }

        // Safe to link - user only has OAuth accounts
        // ...
    }

    // ...
}
```

---

## 9. Тестування

### 9.1 Unit Tests

**Структура:**

```
Tests/
├── Services/
│   ├── Auth/
│   │   ├── OAuthServiceTests.cs
│   │   └── Providers/
│   │       ├── GoogleOAuthProviderTests.cs
│   │       ├── AppleOAuthProviderTests.cs
│   │       └── GitHubOAuthProviderTests.cs
```

**Приклад:**

**Файл:** `Tests/Services/Auth/OAuthServiceTests.cs`

```csharp
public class OAuthServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IOAuthProvider> _mockProvider;
    private readonly OAuthService _service;

    [Fact]
    public async Task AuthenticateWithProvider_ValidToken_ReturnsAuthResponse()
    {
        // Arrange
        var userInfo = new OAuthUserInfo
        {
            Provider = "Google",
            ExternalId = "123456",
            Email = "test@example.com",
            EmailVerified = true
        };

        _mockProvider.Setup(p => p.ValidateTokenAsync(It.IsAny<string>()))
            .ReturnsAsync(userInfo);

        // Act
        var result = await _service.AuthenticateWithProviderAsync("Google", "id_token");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task FindOrCreateUser_ExistingUser_ReturnsUser()
    {
        // Test find existing user by provider + externalId
    }

    [Fact]
    public async Task FindOrCreateUser_NewUser_CreatesUser()
    {
        // Test creating new OAuth user
    }

    [Fact]
    public async Task FindOrCreateUser_EmailExists_LinksProvider()
    {
        // Test linking OAuth to existing email
    }
}
```

### 9.2 Integration Tests

**Тестувати реальні OAuth провайдери:**

```csharp
public class GoogleOAuthProviderIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task ValidateToken_RealGoogleToken_Success()
    {
        // Use real Google ID token (from test account)
        // Validate it works correctly

        // NOTE: This requires test credentials and real tokens
        // Consider using test mode or mock server
    }
}
```

### 9.3 Manual Testing Checklist

- [ ] Google Sign In works on web
- [ ] Google Sign In works on mobile
- [ ] Apple Sign In works on web
- [ ] Apple Sign In works on iOS
- [ ] GitHub Sign In works
- [ ] Existing user can link OAuth provider
- [ ] Existing user can unlink OAuth provider
- [ ] OAuth user can't login with password
- [ ] Local user can't be auto-linked if has password
- [ ] Email verified automatically for OAuth users
- [ ] CSRF protection works (invalid state rejected)
- [ ] Expired tokens rejected
- [ ] Invalid tokens rejected
- [ ] Rate limiting works

---

## 10. Deployment та конфігурація

### 10.1 Environment Variables

**Production .env:**

```env
# Database (existing)
POSTGRES_HOST=your-db-host
POSTGRES_PORT=5432
POSTGRES_DATABASE=komunalka_prod
POSTGRES_USERNAME=komunalka_user
POSTGRES_PASSWORD=your-secure-password

# JWT (existing)
JWT_SECRET=your-jwt-secret-min-32-chars
JWT_ISSUER=https://api.yourdomain.com
JWT_AUDIENCE=https://yourdomain.com
JWT_EXPIRATION_MINUTES=30
JWT_REFRESH_TOKEN_EXPIRATION_DAYS=7

# Google OAuth
GOOGLE_CLIENT_ID=your-prod-client-id.apps.googleusercontent.com
GOOGLE_CLIENT_SECRET=your-prod-client-secret
GOOGLE_REDIRECT_URI=https://api.yourdomain.com/api/v1/auth/oauth/callback

# Apple Sign In
APPLE_CLIENT_ID=com.yourdomain.app
APPLE_TEAM_ID=YOUR_TEAM_ID
APPLE_KEY_ID=YOUR_KEY_ID
APPLE_PRIVATE_KEY_PATH=/app/secrets/AuthKey_XXXXX.p8
APPLE_REDIRECT_URI=https://api.yourdomain.com/api/v1/auth/oauth/callback

# GitHub OAuth
GITHUB_CLIENT_ID=your-prod-github-client-id
GITHUB_CLIENT_SECRET=your-prod-github-client-secret
GITHUB_REDIRECT_URI=https://api.yourdomain.com/api/v1/auth/oauth/callback

# Rate Limiting
OAUTH_RATE_LIMIT_REQUESTS=10
OAUTH_RATE_LIMIT_WINDOW_MINUTES=1

# Security
DATA_PROTECTION_KEY_PATH=/app/secrets/data-protection-keys
```

### 10.2 Docker Configuration

**docker-compose.yml:**

```yaml
services:
  api:
    build: .
    ports:
      - "8080:8080"
      - "8081:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
    env_file:
      - .env
    volumes:
      - ./secrets:/app/secrets:ro  # Mount Apple private key
      - data-protection-keys:/app/secrets/data-protection-keys
    depends_on:
      - postgres

  postgres:
    image: postgres:17
    # ... existing config

volumes:
  data-protection-keys:
```

### 10.3 CI/CD Considerations

**Secrets Management:**

1. Use environment-specific secrets (dev, staging, prod)
2. Never commit secrets to git
3. Use secret management service (AWS Secrets Manager, Azure Key Vault)
4. Rotate secrets regularly

**Deployment Steps:**

```bash
# 1. Pull latest code
git pull origin main

# 2. Set environment variables
export $(cat .env.production | xargs)

# 3. Build Docker image
docker-compose build

# 4. Run migrations
docker-compose run --rm api dotnet ef database update

# 5. Start services
docker-compose up -d

# 6. Verify
curl https://api.yourdomain.com/health
```

### 10.4 Monitoring

**Metrics to track:**

- OAuth login success rate
- OAuth login failures (by provider)
- Token validation failures
- Average authentication time
- New OAuth users created
- OAuth providers linked/unlinked

**Logging:**

```csharp
_logger.LogInformation(
    "OAuth authentication successful: Provider={Provider}, ExternalId={ExternalId}, Email={Email}",
    userInfo.Provider,
    userInfo.ExternalId,
    userInfo.Email);

_logger.LogWarning(
    "OAuth token validation failed: Provider={Provider}, Reason={Reason}",
    provider,
    exception.Message);
```

---

## 11. Implementation Checklist

### Phase 1: Foundation (Days 1-2)
- [ ] Update User model with OAuth fields
- [ ] Create migration AddOAuthSupport
- [ ] Create OAuthUserInfo DTO
- [ ] Create OAuthLoginRequest/Response DTOs
- [ ] Update AuthenticationResponse DTO

### Phase 2: Google OAuth (Days 3-4)
- [ ] Install Google.Apis.Auth NuGet package
- [ ] Create IOAuthProvider interface
- [ ] Implement GoogleOAuthProvider
- [ ] Create IOAuthService interface
- [ ] Implement OAuthService (basic)
- [ ] Add Google endpoints to AuthController
- [ ] Configure Google OAuth in Program.cs
- [ ] Test Google Sign In

### Phase 3: Apple Sign In (Days 5-6)
- [ ] Install AspNet.Security.OAuth.Apple package
- [ ] Implement AppleOAuthProvider
- [ ] Configure Apple Developer account
- [ ] Add Apple private key handling
- [ ] Add Apple endpoints
- [ ] Test Apple Sign In

### Phase 4: GitHub OAuth (Day 7)
- [ ] Install AspNet.Security.OAuth.GitHub package
- [ ] Implement GitHubOAuthProvider
- [ ] Add GitHub endpoints
- [ ] Test GitHub Sign In

### Phase 5: Security & Polish (Days 8-9)
- [ ] Implement CSRF protection (state parameter)
- [ ] Add rate limiting
- [ ] Add token encryption (if storing)
- [ ] Implement account takeover prevention
- [ ] Add comprehensive logging
- [ ] Update documentation

### Phase 6: Testing (Day 10)
- [ ] Write unit tests for OAuthService
- [ ] Write unit tests for each provider
- [ ] Write integration tests
- [ ] Manual testing all flows
- [ ] Security audit

### Phase 7: Deployment (Day 11)
- [ ] Update docker-compose.yml
- [ ] Configure production environment variables
- [ ] Deploy to staging
- [ ] Test on staging
- [ ] Deploy to production

---

## 12. FAQ & Troubleshooting

### Q: Чи потрібно зберігати OAuth access tokens?

**A:** Залежить від use case:
- ❌ Якщо тільки автентифікація - НІ
- ✅ Якщо потрібен доступ до API провайдера (Google Drive, GitHub repos) - ТАК, але encrypted

### Q: Як обробити випадок коли email змінюється у провайдера?

**A:** Email від OAuth провайдера може змінитись. Рекомендації:
1. Використовувати ExternalId (sub claim) як primary key
2. Email - вторинний ідентифікатор
3. При зміні email - оновити в БД

### Q: Що робити якщо користувач видалить акаунт у провайдера?

**A:** Token validation провалиться. Користувач не зможе залогінитись. Потрібно:
1. Дозволити користувачу додати password (конвертувати в local account)
2. Або додати інший OAuth provider

### Q: Як тестувати Apple Sign In без Apple Developer account?

**A:** Використати mock:
```csharp
public class MockAppleOAuthProvider : IOAuthProvider
{
    public async Task<OAuthUserInfo?> ValidateTokenAsync(string token)
    {
        // Return fake user info for testing
    }
}
```

### Q: Чи потрібно зберігати refresh tokens від провайдерів?

**A:** Наша система має власні refresh tokens. OAuth refresh tokens від провайдерів потрібні тільки якщо плануєте доступ до їх API.

---

## 13. Майбутні покращення

### v2.0 Features:
- [ ] Microsoft OAuth
- [ ] Facebook Login
- [ ] Multi-factor authentication (MFA)
- [ ] Social profile sync (avatar, name updates)
- [ ] Account merging (multiple providers → one account)
- [ ] OAuth token management UI

### v3.0 Features:
- [ ] Passwordless authentication (Magic Links)
- [ ] Biometric authentication
- [ ] SSO (Single Sign-On) support
- [ ] Admin dashboard for OAuth analytics

---

## 14. Посилання

### Documentation:
- [Google Sign-In](https://developers.google.com/identity/sign-in/web)
- [Apple Sign In](https://developer.apple.com/sign-in-with-apple/)
- [GitHub OAuth](https://docs.github.com/en/developers/apps/building-oauth-apps)
- [OAuth 2.0 RFC](https://datatracker.ietf.org/doc/html/rfc6749)
- [OpenID Connect](https://openid.net/connect/)

### Libraries:
- [Google.Apis.Auth](https://www.nuget.org/packages/Google.Apis.Auth)
- [AspNet.Security.OAuth.Apple](https://www.nuget.org/packages/AspNet.Security.OAuth.Apple)
- [AspNet.Security.OAuth.GitHub](https://www.nuget.org/packages/AspNet.Security.OAuth.GitHub)

---

**Оновлення плану:** 2025-12-12
**Автор:** Oleksii + Claude
**Версія:** 1.0
