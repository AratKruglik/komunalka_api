using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.User;

using Models;

public interface IUserRepository : IRepository<User>
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<EntityEntry<User?>> AddAsync(User? user);
    void Update(User user);
    void Delete(User user);
}
