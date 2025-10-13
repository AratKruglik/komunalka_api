using KomunalkaAPI.Models;

namespace KomunalkaAPI.Repositories.RefreshToken;

/// <summary>
/// Repository interface for working with refresh tokens
/// </summary>
public interface IRefreshTokenRepository : IRepository<Models.RefreshToken>
{
    /// <summary>
    /// Get refresh token by token value
    /// </summary>
    /// <param name="token">Token value</param>
    /// <returns>Refresh token or null</returns>
    Task<Models.RefreshToken?> GetByTokenAsync(string token);

    /// <summary>
    /// Get refresh token by token value with included user
    /// </summary>
    /// <param name="token">Token value</param>
    /// <returns>Refresh token with user or null</returns>
    Task<Models.RefreshToken?> GetByTokenWithUserAsync(string token);

    /// <summary>
    /// Get all active refresh tokens for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of active tokens</returns>
    Task<IEnumerable<Models.RefreshToken>> GetActiveTokensByUserIdAsync(int userId);

    /// <summary>
    /// Delete expired refresh tokens
    /// </summary>
    /// <returns>Number of deleted tokens</returns>
    Task<int> DeleteExpiredTokensAsync();
}
