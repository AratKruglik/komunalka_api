using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Address;

using Models;

public interface IAddressRepository
{
    public new Task<IEnumerable<Address>> GetAllAsync();
    public new Task<Address?> GetByIdAsync(int id);
    public new Task<IEnumerable<Address>> GetWithDeletedAsync();
    public new Task<EntityEntry<Address>> AddAsync(Address address);
    public new void Update(Address address);
    public new void Delete(Address address);
}
