using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Address;

using Models;

public class AddressRepository(DbContext context) : Repository<Address>(context), IAddressRepository
{
    public override async Task<IEnumerable<Address>> GetAllAsync()
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Region)
            .Include(a => a.AddressType)
            .Where(a => a.DeletedAt == null)
            .ToListAsync();
    }

    public override async Task<Address?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Region)
            .Include(a => a.AddressType)
            .Where(a => a.DeletedAt == null)
            .FirstOrDefaultAsync(address => address.Id == id);
    }

    public async Task<IEnumerable<Address>> GetWithDeletedAsync()
    {
        return await _dbSet
            .Include(a => a.User)
            .Include(a => a.Region)
            .Include(a => a.AddressType)
            .ToListAsync();
    }

    public override async Task<EntityEntry<Address>> AddAsync(Address address)
    {
        return await _dbSet.AddAsync(address);
    }

    public override void Update(Address address)
    {
        _dbSet.Update(address);
    }

    public override void Delete(Address address)
    {
        _dbSet.Remove(address);
    }

    public async Task<List<Address>> GetByUserIdAsync(
        int userId,
        int skip,
        int take,
        string? sortBy,
        bool desc,
        bool includeDeps,
        CancellationToken cancellationToken)
    {
        IQueryable<Address> query = _dbSet.Where(a => a.DeletedAt == null && a.UserId == userId);

        if (includeDeps)
        {
            query = query
                .Include(a => a.Region)
                .Include(a => a.AddressType);
        }

        query = (sortBy?.ToLowerInvariant()) switch
        {
            "city" => desc ? query.OrderByDescending(a => a.City) : query.OrderBy(a => a.City),
            "updatedat" => desc ? query.OrderByDescending(a => a.UpdatedAt) : query.OrderBy(a => a.UpdatedAt),
            "isprimary" => desc ? query.OrderByDescending(a => a.IsPrimary).ThenByDescending(a => a.UpdatedAt) : query.OrderBy(a => a.IsPrimary).ThenBy(a => a.UpdatedAt),
            _ => desc ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt)
        };

        if (skip > 0) query = query.Skip(skip);
        if (take > 0) query = query.Take(take);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await _dbSet.CountAsync(a => a.DeletedAt == null && a.UserId == userId, cancellationToken);
    }

    public async Task<List<Address>> GetUserAddressesAsync(int userId, CancellationToken cancellationToken)
    {
        return await _dbSet.Where(a => a.DeletedAt == null && a.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
