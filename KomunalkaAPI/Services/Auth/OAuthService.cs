using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Auth.Providers;

namespace KomunalkaAPI.Services.Auth;

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

        var provider = GetProvider(providerName);
        if (provider == null)
        {
            _logger.LogWarning("OAuth provider not found: {Provider}", providerName);
            return null;
        }

        var userInfo = await provider.ValidateTokenAsync(token);
        if (userInfo == null)
        {
            _logger.LogWarning(
                "OAuth token validation failed for provider: {Provider}",
                providerName);
            return null;
        }

        var user = await FindOrCreateUserAsync(userInfo);
        if (user == null)
        {
            _logger.LogWarning(
                "Failed to find or create user for OAuth provider: {Provider}, Email: {Email}",
                providerName,
                userInfo.Email);
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation(
            "OAuth authentication successful for user: {UserId}, Provider: {Provider}",
            user.Id,
            providerName);

        return await GenerateAuthenticationResponseAsync(user);
    }

    public async Task<AuthenticationResponse?> HandleCallbackAsync(OAuthCallbackRequest request)
    {
        _logger.LogInformation(
            "OAuth callback received for provider: {Provider}",
            request.Provider);

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

        var provider = GetProvider(request.Provider);
        if (provider == null)
        {
            _logger.LogWarning("OAuth provider not found: {Provider}", request.Provider);
            return null;
        }

        var userInfo = await provider.ExchangeCodeAsync(request.Code);
        if (userInfo == null)
        {
            _logger.LogWarning(
                "OAuth code exchange failed for provider: {Provider}",
                request.Provider);
            return null;
        }

        var user = await FindOrCreateUserAsync(userInfo);
        if (user == null)
        {
            return null;
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.CompleteAsync();

        return await GenerateAuthenticationResponseAsync(user);
    }

    public string GetAuthorizationUrl(string providerName, string? redirectUri = null)
    {
        var provider = GetProvider(providerName);
        if (provider == null)
        {
            throw new ArgumentException($"OAuth provider not found: {providerName}");
        }

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

        if (user.Password == null)
        {
            _logger.LogWarning(
                "Cannot unlink OAuth provider - user has no password set");
            return false;
        }

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

    private async Task<User?> FindOrCreateUserAsync(OAuthUserInfo userInfo)
    {
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

        user = await _unitOfWork.Users.GetByEmailAsync(userInfo.Email);

        if (user != null)
        {
            if (user.Password != null)
            {
                _logger.LogWarning(
                    "User with email {Email} already exists with password. Manual linking required.",
                    userInfo.Email);
                return null;
            }

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
            Password = null,
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

    private async Task<string> GenerateUniqueUsernameAsync(OAuthUserInfo userInfo)
    {
        if (!string.IsNullOrEmpty(userInfo.Username))
        {
            var baseUsername = userInfo.Username.Replace(" ", "").ToLower();
            return await EnsureUniqueUsernameAsync(baseUsername);
        }

        var emailUsername = userInfo.Email.Split('@')[0].Replace(".", "").Replace("+", "");
        return await EnsureUniqueUsernameAsync(emailUsername);
    }

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

    private IOAuthProvider? GetProvider(string providerName)
    {
        return _oauthProviders.FirstOrDefault(
            p => p.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }

    private static string GenerateState()
    {
        return Convert.ToBase64String(
            System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
    }
}
