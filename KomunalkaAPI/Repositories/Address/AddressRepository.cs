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
}
