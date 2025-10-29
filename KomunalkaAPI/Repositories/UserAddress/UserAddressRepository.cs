using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.UserAddress;

using Models;

public class UserAddressRepository(DbContext context) : Repository<UserAddress>(context), IUserAddressRepository
{
    public override async Task<IEnumerable<UserAddress>> GetAllAsync()
    {
        return await _dbSet
            .Include(ua => ua.User)
            .Include(ua => ua.Address)
                .ThenInclude(a => a.Region)
            .Include(ua => ua.Address)
                .ThenInclude(a => a.AddressType)
            .ToListAsync();
    }

    public override async Task<UserAddress?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(ua => ua.User)
            .Include(ua => ua.Address)
                .ThenInclude(a => a.Region)
            .Include(ua => ua.Address)
                .ThenInclude(a => a.AddressType)
            .FirstOrDefaultAsync(ua => ua.Id == id);
    }

    public override async Task<EntityEntry<UserAddress>> AddAsync(UserAddress userAddress)
    {
        return await _dbSet.AddAsync(userAddress);
    }

    public override void Update(UserAddress userAddress)
    {
        _dbSet.Update(userAddress);
    }

    public override void Delete(UserAddress userAddress)
    {
        _dbSet.Remove(userAddress);
    }

    public async Task<List<UserAddress>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ua => ua.Address)
                .ThenInclude(a => a.Region)
            .Include(ua => ua.Address)
                .ThenInclude(a => a.AddressType)
            .Where(ua => ua.UserId == userId && ua.Address.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserAddress?> GetByUserAndAddressAsync(int userId, int addressId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ua => ua.Address)
                .ThenInclude(a => a.Region)
            .Include(ua => ua.Address)
                .ThenInclude(a => a.AddressType)
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AddressId == addressId, cancellationToken);
    }

    public async Task<UserAddress?> GetPrimaryByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(ua => ua.Address)
                .ThenInclude(a => a.Region)
            .Include(ua => ua.Address)
                .ThenInclude(a => a.AddressType)
            .Where(ua => ua.UserId == userId && ua.IsPrimary && ua.Address.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task ResetPrimaryForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userAddresses = await _dbSet
            .Where(ua => ua.UserId == userId && ua.IsPrimary)
            .ToListAsync(cancellationToken);

        foreach (var userAddress in userAddresses)
        {
            userAddress.IsPrimary = false;
            userAddress.UpdatedAt = DateTime.UtcNow;
        }
    }

    public async Task<bool> UserHasAccessToAddressAsync(int userId, int addressId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AnyAsync(ua => ua.UserId == userId && ua.AddressId == addressId, cancellationToken);
    }
}
