using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.AddressType;

public class AddressTypeRepository(DbContext context) : Repository<Models.AddressType>(context), IAddressTypeRepository
{
    public new async Task<IEnumerable<Models.AddressType>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public new Task<Models.AddressType?> GetByIdAsync(int id)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public new async Task<EntityEntry<Models.AddressType>> AddAsync(Models.AddressType addressType)
    {
        return await _dbSet.AddAsync(addressType);
    }

    public new void Update(Models.AddressType addressType)
    {
        _dbSet.Update(addressType);
    }

    public new void Delete(Models.AddressType addressType)
    {
        _dbSet.Remove(addressType);
    }
}
