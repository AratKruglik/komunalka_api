using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.RefreshToken;

/// <summary>
/// Repository for working with refresh tokens
/// </summary>
public class RefreshTokenRepository(DbContext context)
    : Repository<Models.RefreshToken>(context), IRefreshTokenRepository
{
    public async Task<Models.RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<Models.RefreshToken?> GetByTokenWithUserAsync(string token)
    {
        return await _dbSet
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<IEnumerable<Models.RefreshToken>> GetActiveTokensByUserIdAsync(int userId)
    {
        return await _dbSet
            .Where(rt => rt.UserId == userId
                         && !rt.IsRevoked
                         && !rt.IsUsed
                         && rt.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();
    }

    public async Task<int> DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _dbSet
            .Where(rt => rt.ExpiryDate < DateTime.UtcNow || rt.IsRevoked || rt.IsUsed)
            .ToListAsync();

        _dbSet.RemoveRange(expiredTokens);

        return expiredTokens.Count;
    }
}
