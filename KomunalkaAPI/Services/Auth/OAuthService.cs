using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Auth.Providers;

namespace KomunalkaAPI.Services.Auth;

/// <summary>
/// OAuth authentication service implementation
/// Coordinates OAuth providers and user management
/// </summary>
public class OAuthService : IOAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IEnumerable<IOAuthProvider> _oauthProviders;
    private readonly ILogger<OAuthService> _logger;

    public OAuthService(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IEnumerable<IOAuthProvider> oauthProviders,
        ILogger<OAuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _oauthProviders = oauthProviders;
        _logger = logger;
    }

    public async Task<AuthenticationResponse?> AuthenticateWithProviderAsync(
        string providerName,
        string token)
    {
        _logger.LogInformation(
            "OAuth authentication attempt with provider: {Provider}",
            providerName);

        // Find appropriate provider
        var provider = GetProvider(providerName);
        if (provider == null)
        {
            _logger.LogWarning("OAuth provider not found: {Provider}", providerName);
            return null;
        }

        // Validate token and get user info
        var userInfo = await provider.ValidateTokenAsync(token);
        if (userInfo == null)
        {
            _logger.LogWarning(
                "OAuth token validation failed for provider: {Provider}",
                providerName);
            return null;
        }

        // Find or create user
        var user = await FindOrCreateUserAsync(userInfo);
        if (user == null)
        {
            _logger.LogWarning(
                "Failed to find or create user for OAuth provider: {Provider}, Email: {Email}",
                providerName,
                userInfo.Email);
            return null;
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "OAuth authentication successful for user: {UserId}, Provider: {Provider}",
            user.Id,
            providerName);

        // Generate JWT and refresh token
        return await GenerateAuthenticationResponseAsync(user);
    }

    public async Task<AuthenticationResponse?> HandleCallbackAsync(OAuthCallbackRequest request)
    {
        _logger.LogInformation(
            "OAuth callback received for provider: {Provider}",
            request.Provider);

        // Check for errors
        if (!string.IsNullOrEmpty(request.Error))
        {
            _logger.LogWarning(
                "OAuth callback error: {Error}, Description: {Description}",
                request.Error,
                request.ErrorDescription);
            return null;
        }

        if (string.IsNullOrEmpty(request.Code))
        {
            _logger.LogWarning("OAuth callback missing authorization code");
            return null;
        }

        // Find appropriate provider
        var provider = GetProvider(request.Provider);
        if (provider == null)
        {
            _logger.LogWarning("OAuth provider not found: {Provider}", request.Provider);
            return null;
        }

        // Exchange code for tokens
        var userInfo = await provider.ExchangeCodeAsync(request.Code);
        if (userInfo == null)
        {
            _logger.LogWarning(
                "OAuth code exchange failed for provider: {Provider}",
                request.Provider);
            return null;
        }

        // Find or create user
        var user = await FindOrCreateUserAsync(userInfo);
        if (user == null)
        {
            return null;
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync();

        // Generate JWT and refresh token
        return await GenerateAuthenticationResponseAsync(user);
    }

    public string GetAuthorizationUrl(string providerName, string? redirectUri = null)
    {
        var provider = GetProvider(providerName);
        if (provider == null)
        {
            throw new ArgumentException($"OAuth provider not found: {providerName}");
        }

        // Generate state for CSRF protection
        var state = GenerateState();

        return provider.GetAuthorizationUrl(state, redirectUri);
    }

    public async Task<bool> LinkProviderAsync(int userId, string providerName, string token)
    {
        _logger.LogInformation(
            "Linking OAuth provider to user: {UserId}, Provider: {Provider}",
            userId,
            providerName);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return false;
        }

        var provider = GetProvider(providerName);
        if (provider == null)
        {
            _logger.LogWarning("OAuth provider not found: {Provider}", providerName);
            return false;
        }

        var userInfo = await provider.ValidateTokenAsync(token);
        if (userInfo == null)
        {
            _logger.LogWarning("OAuth token validation failed");
            return false;
        }

        // Check if this OAuth account is already linked to another user
        var existingUser = await _unitOfWork.Users.GetByProviderAsync(
            userInfo.Provider,
            userInfo.ExternalId);

        if (existingUser != null && existingUser.Id != userId)
        {
            _logger.LogWarning(
                "OAuth account already linked to another user: {ExistingUserId}",
                existingUser.Id);
            return false;
        }

        // Link OAuth to user
        user.AuthProvider = userInfo.Provider;
        user.ExternalId = userInfo.ExternalId;
        user.EmailVerified = userInfo.EmailVerified || user.EmailVerified;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "OAuth provider linked successfully: {UserId}, Provider: {Provider}",
            userId,
            providerName);

        return true;
    }

    public async Task<bool> UnlinkProviderAsync(int userId, string providerName)
    {
        _logger.LogInformation(
            "Unlinking OAuth provider from user: {UserId}, Provider: {Provider}",
            userId,
            providerName);

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return false;
        }

        if (user.AuthProvider != providerName)
        {
            _logger.LogWarning(
                "User is not linked to this provider: {Provider}",
                providerName);
            return false;
        }

        // Don't allow unlinking if user has no password (would lock them out)
        if (user.Password == null)
        {
            _logger.LogWarning(
                "Cannot unlink OAuth provider - user has no password set");
            return false;
        }

        // Unlink provider
        user.AuthProvider = "Local";
        user.ExternalId = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "OAuth provider unlinked successfully: {UserId}, Provider: {Provider}",
            userId,
            providerName);

        return true;
    }

    /// <summary>
    /// Find or create user from OAuth user info
    /// </summary>
    private async Task<User?> FindOrCreateUserAsync(OAuthUserInfo userInfo)
    {
        // 1. Try to find by provider + externalId
        var user = await _unitOfWork.Users.GetByProviderAsync(
            userInfo.Provider,
            userInfo.ExternalId);

        if (user != null)
        {
            _logger.LogInformation(
                "Found existing user by OAuth provider: {UserId}",
                user.Id);
            return user;
        }

        // 2. Try to find by email
        user = await _unitOfWork.Users.GetByEmailAsync(userInfo.Email);

        if (user != null)
        {
            // User exists with this email
            // Only auto-link if user has no password (OAuth-only account)
            if (user.Password != null)
            {
                _logger.LogWarning(
                    "User with email {Email} already exists with password. Manual linking required.",
                    userInfo.Email);
                return null;
            }

            // Safe to auto-link - user only has OAuth accounts
            _logger.LogInformation(
                "Auto-linking OAuth provider to existing user: {UserId}",
                user.Id);

            user.AuthProvider = userInfo.Provider;
            user.ExternalId = userInfo.ExternalId;
            user.EmailVerified = userInfo.EmailVerified;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();

            return user;
        }

        // 3. Create new user
        _logger.LogInformation(
            "Creating new user from OAuth provider: {Provider}, Email: {Email}",
            userInfo.Provider,
            userInfo.Email);

        var newUser = new User
        {
            Username = await GenerateUniqueUsernameAsync(userInfo),
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

        _logger.LogInformation(
            "New user created from OAuth: {UserId}, Provider: {Provider}",
            newUser.Id,
            userInfo.Provider);

        return newUser;
    }

    /// <summary>
    /// Generate unique username from OAuth user info
    /// </summary>
    private async Task<string> GenerateUniqueUsernameAsync(OAuthUserInfo userInfo)
    {
        // Try to use provider username first
        if (!string.IsNullOrEmpty(userInfo.Username))
        {
            var baseUsername = userInfo.Username.Replace(" ", "").ToLower();
            return await EnsureUniqueUsernameAsync(baseUsername);
        }

        // Try email username part
        var emailUsername = userInfo.Email.Split('@')[0].Replace(".", "").Replace("+", "");
        return await EnsureUniqueUsernameAsync(emailUsername);
    }

    /// <summary>
    /// Ensure username is unique by appending number if needed
    /// </summary>
    private async Task<string> EnsureUniqueUsernameAsync(string baseUsername)
    {
        var username = baseUsername;
        var counter = 1;

        while (true)
        {
            var existingUsers = await _unitOfWork.Users.GetAllAsync();
            if (!existingUsers.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                return username;
            }

            counter++;
            username = $"{baseUsername}{counter}";
        }
    }

    /// <summary>
    /// Generate authentication response with JWT tokens
    /// </summary>
    private async Task<AuthenticationResponse> GenerateAuthenticationResponseAsync(User user)
    {
        var token = _jwtService.GenerateJwtToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken(user);

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.CompleteAsync();

        return new AuthenticationResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            RefreshToken = refreshToken.Token,
            Expiration = _jwtService.GetTokenExpirationTime(token),
            AuthProvider = user.AuthProvider,
            EmailVerified = user.EmailVerified
        };
    }

    /// <summary>
    /// Get OAuth provider by name
    /// </summary>
    private IOAuthProvider? GetProvider(string providerName)
    {
        return _oauthProviders.FirstOrDefault(
            p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Generate random state for CSRF protection
    /// </summary>
    private static string GenerateState()
    {
        return Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
    }
}
