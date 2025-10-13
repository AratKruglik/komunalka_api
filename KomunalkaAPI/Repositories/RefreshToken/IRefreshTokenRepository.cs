using KomunalkaAPI.Models;

namespace KomunalkaAPI.Repositories.RefreshToken;

/// <summary>
/// Інтерфейс репозиторію для роботи з токенами оновлення
/// </summary>
public interface IRefreshTokenRepository : IRepository<Models.RefreshToken>
{
    /// <summary>
    /// Отримати токен оновлення за значенням токена
    /// </summary>
    /// <param name="token">Значення токена</param>
    /// <returns>Токен оновлення або null</returns>
    Task<Models.RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Отримати токен оновлення за значенням токена з включеним користувачем
    /// </summary>
    /// <param name="token">Значення токена</param>
    /// <returns>Токен оновлення з користувачем або null</returns>
    Task<Models.RefreshToken?> GetByTokenWithUserAsync(string token);

    /// <summary>
    /// Отримати всі активні токени оновлення користувача
    /// </summary>
    /// <param name="userId">ID користувача</param>
    /// <returns>Список активних токенів</returns>
    Task<IEnumerable<Models.RefreshToken>> GetActiveTokensByUserIdAsync(int userId);

    /// <summary>
    /// Видалити прострочені токени оновлення
    /// </summary>
    /// <returns>Кількість видалених токенів</returns>
    Task<int> DeleteExpiredTokensAsync();
}
