using System.Security.Claims;
using KomunalkaAPI.Data;
using KomunalkaAPI.DTO.Auth;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Repositories.User;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Services.Auth;

public class AuthService(
    IUnitOfWork unitOfWork,
    IUserRepository userRepository,
    IJwtService jwtService,
    ApplicationDbContext dbContext)
    : IAuthService
{
    public async Task<AuthenticationResponse?> AuthenticateAsync(AuthenticationRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);

        if (user == null || !VerifyPassword(request.Password, user.Password))
        {
            return null;
        }

        return await GenerateAuthenticationResponseAsync(user);
    }

    public async Task<AuthenticationResponse?> RegisterAsync(RegisterUserRequest request)
    {
        // Перевірка, чи існує користувач з таким email
        var existingUser = await userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
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

        await userRepository.AddAsync(newUser);
        await unitOfWork.CompleteAsync();

        return await GenerateAuthenticationResponseAsync(newUser);
    }

    public async Task<AuthenticationResponse?> RefreshTokenAsync(string refreshToken)
    {
        var storedToken = await GetValidRefreshTokenAsync(refreshToken);

        if (storedToken == null)
        {
            return null;
        }

        // Позначаємо старий токен як використаний
        storedToken.IsUsed = true;
        dbContext.RefreshTokens.Update(storedToken);
        await dbContext.SaveChangesAsync();

        // Генеруємо новий токен та відповідь
        return await GenerateAuthenticationResponseAsync(storedToken.User);
    }

    private async Task<RefreshToken?> GetValidRefreshTokenAsync(string refreshToken)
    {
        var storedToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken == null || 
            storedToken.IsUsed || 
            storedToken.IsRevoked || 
            storedToken.ExpiryDate < DateTime.UtcNow || 
            storedToken.User == null)
        {
            return null;
        }

        return storedToken;
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var storedToken = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (storedToken == null)
        {
            return false;
        }

        storedToken.IsRevoked = true;
        dbContext.RefreshTokens.Update(storedToken);
        await dbContext.SaveChangesAsync();

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

        var user = await userRepository.GetByIdAsync(userId);
        return user != null;
    }

    private async Task<AuthenticationResponse> GenerateAuthenticationResponseAsync(User? user)
    {
        var token = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user);

        // Зберігаємо токен оновлення в базі даних
        await dbContext.RefreshTokens.AddAsync(refreshToken);
        await dbContext.SaveChangesAsync();

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
        // У реальному проекті використовуйте BCrypt або Argon2
        // Для простоти цього прикладу використовуємо базовий хеш
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string passwordHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}
