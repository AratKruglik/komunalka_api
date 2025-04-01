using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Address;

using Models;

public class AddressRepository(DbContext context) : Repository<Address>(context), IAddressRepository
{
    public new async Task<IEnumerable<Address>> GetAllAsync()
    {
        return await _dbSet
            .Include(a => a.User)
            .Where(a => a.DeletedAt == null)
            .ToListAsync();
    }

    public new async Task<Address?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(a => a.User)
            .Where(a => a.DeletedAt == null)
            .FirstOrDefaultAsync(address => address.Id == id);
    }
    
    public new async Task<IEnumerable<Address>> GetWithDeletedAsync()
    {
        return await _dbSet
            .Include(a => a.User)
            .ToListAsync();
    }

    public new async Task<EntityEntry<Address>> AddAsync(Address address)
    {
        return await _dbSet.AddAsync(address);
    }

    public new void Update(Address address)
    {
        _dbSet.Update(address);
    }

    public new void Delete(Address address)
    {
        _dbSet.Remove(address);
    }
}
