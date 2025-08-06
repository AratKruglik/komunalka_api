using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Address;

using Models;

public interface IAddressRepository : IRepository<Address>
{
    Task<IEnumerable<Address>> GetAllAsync();
    Task<Address?> GetByIdAsync(int id);
    Task<IEnumerable<Address>> GetWithDeletedAsync();
    Task<EntityEntry<Address>> AddAsync(Address address);
    void Update(Address address);
    void Delete(Address address);
}
