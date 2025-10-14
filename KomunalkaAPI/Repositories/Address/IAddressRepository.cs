using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Address;

using Models;

public interface IAddressRepository : IRepository<Address>
{
    new Task<IEnumerable<Address>> GetAllAsync();
    new Task<Address?> GetByIdAsync(int id);
    Task<IEnumerable<Address>> GetWithDeletedAsync();
    new Task<EntityEntry<Address>> AddAsync(Address address);
    new void Update(Address address);
    new void Delete(Address address);

    // Query-specific methods
    Task<List<Address>> GetByUserIdAsync(
        int userId,
        int skip,
        int take,
        string? sortBy,
        bool desc,
        bool includeDeps,
        CancellationToken cancellationToken);

    Task<int> CountByUserIdAsync(int userId, CancellationToken cancellationToken);

    Task<List<Address>> GetUserAddressesAsync(int userId, CancellationToken cancellationToken);
}
