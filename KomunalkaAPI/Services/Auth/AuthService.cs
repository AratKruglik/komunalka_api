using System.Security.Claims;
using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;

namespace KomunalkaAPI.Services.Auth;

/// <summary>
/// Сервіс аутентифікації користувачів
/// </summary>
public class AuthService(
    IUnitOfWork unitOfWork,
    IJwtService jwtService,
    ILogger<AuthService> logger)
    : IAuthService
{
    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        logger.LogInformation("Спроба аутентифікації для email: {Email}", request.Email);

        var user = await unitOfWork.Users.GetByEmailAsync(request.Email);

        if (user == null || !VerifyPassword(request.Password, user.Password))
        {
            logger.LogWarning("Невдала спроба аутентифікації для email: {Email}", request.Email);
            return null;
        }

        logger.LogInformation("Успішна аутентифікація користувача: {Username} ({Email})",
            user.Username, user.Email);

        return await GenerateAuthenticationResponseAsync(user);
    }

    public async Task<AuthenticationResponse?> RegisterAsync(RegisterUserRequest request)
    {
        logger.LogInformation("Спроба реєстрації нового користувача: {Email}", request.Email);

        // Перевірка, чи існує користувач з таким email
        var existingUser = await unitOfWork.Users.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            logger.LogWarning("Спроба реєстрації з існуючим email: {Email}", request.Email);
            return null;
        }

        // Створення нового користувача
        var newUser = new User
        {
            Username = request.Username,
            Email = request.Email,
            Password = HashPassword(request.Password),
            Role = "User"
        };

        await unitOfWork.Users.AddAsync(newUser);
        await unitOfWork.CompleteAsync();

        logger.LogInformation("Успішна реєстрація користувача: {Username} ({Email})",
            newUser.Username, newUser.Email);

        return await GenerateAuthenticationResponseAsync(newUser);
    }

    public async Task<AuthenticationResponse?> RefreshTokenAsync(string refreshToken)
    {
        logger.LogInformation("Спроба оновлення токена");

        var storedToken = await unitOfWork.RefreshTokens.GetByTokenWithUserAsync(refreshToken);

        if (storedToken == null ||
            storedToken.IsUsed ||
            storedToken.IsRevoked ||
            storedToken.ExpiryDate < DateTime.UtcNow ||
            storedToken.User == null)
        {
            logger.LogWarning("Невдала спроба оновлення токена: токен недійсний або прострочений");
            return null;
        }

        // Позначаємо старий токен як використаний
        storedToken.IsUsed = true;
        unitOfWork.RefreshTokens.Update(storedToken);
        await unitOfWork.CompleteAsync();

        logger.LogInformation("Успішне оновлення токена для користувача: {Email}",
            storedToken.User.Email);

        // Генеруємо новий токен та відповідь
        return await GenerateAuthenticationResponseAsync(storedToken.User);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        logger.LogInformation("Спроба відкликання токена");

        var storedToken = await unitOfWork.RefreshTokens.GetByTokenAsync(refreshToken);

        if (storedToken == null)
        {
            logger.LogWarning("Спроба відкликання неіснуючого токена");
            return false;
        }

        storedToken.IsRevoked = true;
        unitOfWork.RefreshTokens.Update(storedToken);
        await unitOfWork.CompleteAsync();

        logger.LogInformation("Токен успішно відкликано для користувача ID: {UserId}",
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

        // Зберігаємо токен оновлення в базі даних через Unit of Work
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
            Expiration = jwtService.GetTokenExpirationTime(token)
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
