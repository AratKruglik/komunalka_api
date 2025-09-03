using KomunalkaAPI.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.AddressType;

public interface IAddressTypeRepository : IRepository<Models.AddressType>
{
    new Task<IEnumerable<Models.AddressType>> GetAllAsync();
    new Task<Models.AddressType?> GetByIdAsync(int id);
    new Task<EntityEntry<Models.AddressType>> AddAsync(Models.AddressType addressType);
    new void Update(Models.AddressType addressType);
    new void Delete(Models.AddressType addressType);
}
