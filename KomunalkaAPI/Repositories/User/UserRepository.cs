using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.User;

using Models;

public class UserRepository(DbContext context) : Repository<User>(context), IUserRepository
{
    public new async Task<List<User>> GetAllAsync()
    {
        return await _dbSet
            .Include(user => user.UserAddresses!)
                .ThenInclude(ua => ua.Address)
            .ToListAsync();
    }

    public new async Task<User?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(user => user.UserAddresses!)
                .ThenInclude(ua => ua.Address)
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(user => user.UserAddresses!)
                .ThenInclude(ua => ua.Address)
            .FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<User?> GetByProviderAsync(string authProvider, string externalId)
    {
        return await _dbSet
            .Include(user => user.UserAddresses!)
                .ThenInclude(ua => ua.Address)
            .FirstOrDefaultAsync(user =>
                user.AuthProvider == authProvider &&
                user.ExternalId == externalId);
    }

    public new async Task<EntityEntry<User>> AddAsync(User user)
    {
        return await _dbSet
            .AddAsync(user);
    }
    
    public new void Update(User user)
    {
        _dbSet
            .Update(user);
    }
    
    public new void Delete(User user)
    {
        _dbSet
            .Remove(user);
    }
}
