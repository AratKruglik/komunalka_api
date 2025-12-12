using System.Security.Claims;
using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;

namespace KomunalkaAPI.Services.Auth;

/// <summary>
/// User authentication service
/// </summary>
public class AuthService(
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        logger.LogInformation("Authentication attempt for email: {Email}", request.Email);

        var user = await unitOfWork.Users.GetByEmailAsync(request.Email);

        if (user == null)
        {
            logger.LogWarning("Failed authentication attempt for email: {Email}", request.Email);
            return null;
        }

        // OAuth users cannot login with password
        if (user.Password == null)
        {
            logger.LogWarning("OAuth user attempted password login: {Email}", request.Email);
            return null;
        }

        if (!VerifyPassword(request.Password, user.Password))
        {
            logger.LogWarning("Failed authentication attempt for email: {Email}", request.Email);
            return null;
        }

        logger.LogInformation("Successful authentication for user: {Username} ({Email})",
            user.Username, user.Email);

        return await GenerateAuthenticationResponseAsync(user);
    }

    public async Task<AuthenticationResponse?> RegisterAsync(RegisterUserRequest request)
    {
        logger.LogInformation("Registration attempt for new user: {Email}", request.Email);

        // Check if user with this email already exists
        var existingUser = await unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Registration attempt with existing email: {Email}", request.Email);
            return null;
        }

        // Create new user
        var newUser = new User
        {
            Username = request.Username,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Password = HashPassword(request.Password),
            Role = "User",
            AuthProvider = "Local",
            EmailVerified = false
        };

        await unitOfWork.Users.AddAsync(newUser);
        await unitOfWork.CompleteAsync();

        logger.LogInformation("Successful registration for user: {Username} ({Email})",
            newUser.Username, newUser.Email);

        return await GenerateAuthenticationResponseAsync(newUser);
    }

    public async Task<AuthenticationResponse?> RefreshTokenAsync(string refreshToken)
    {
        logger.LogInformation("Token refresh attempt");

        var storedToken = await unitOfWork.RefreshTokens.GetByTokenWithUserAsync(refreshToken);

        if (storedToken == null ||
            storedToken.IsUsed ||
            storedToken.IsRevoked ||
            storedToken.ExpiryDate < DateTime.UtcNow ||
            storedToken.User == null)
        {
            logger.LogWarning("Failed token refresh attempt: token is invalid or expired");
            return null;
        }

        // Mark old token as used
        storedToken.IsUsed = true;
        unitOfWork.RefreshTokens.Update(storedToken);
        await unitOfWork.CompleteAsync();

        logger.LogInformation("Successful token refresh for user: {Email}",
            storedToken.User.Email);

        // Generate new token and response
        return await GenerateAuthenticationResponseAsync(storedToken.User);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        logger.LogInformation("Token revocation attempt");

        var storedToken = await unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (storedToken == null)
        {
            logger.LogWarning("Attempt to revoke non-existent token");
            return false;
        }

        storedToken.IsRevoked = true;
        unitOfWork.RefreshTokens.Update(storedToken);
        await unitOfWork.CompleteAsync();

        logger.LogInformation("Token successfully revoked for user ID: {UserId}",
            storedToken.UserId);

        return true;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        var principal = jwtService.GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            return false;
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !int.TryParse(userIdClaim, out var userId))
        {
            return false;
        }

        var user = await unitOfWork.Users.GetByIdAsync(userId);
        return user != null;
    }

    private async Task<AuthenticationResponse> GenerateAuthenticationResponseAsync(User user)
    {
        var token = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);

        // Save refresh token to database through Unit of Work
        await unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await unitOfWork.CompleteAsync();

        return new AuthenticationResponse
        {
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            Token = token,
            RefreshToken = refreshToken.Token,
            Expiration = jwtService.GetTokenExpirationTime(token),
            AuthProvider = user.AuthProvider,
            EmailVerified = user.EmailVerified
        };
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
