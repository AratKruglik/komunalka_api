using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.User;

using Models;

public interface IUserRepository : IRepository<User>
{
    new Task<IEnumerable<User>> GetAllAsync();
    new Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByProviderAsync(string authProvider, string externalId);
    new Task<EntityEntry<User>> AddAsync(User user);
    new void Update(User user);
    new void Delete(User user);
}
