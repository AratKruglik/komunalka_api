using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.ServiceCategory;

using Models;

public interface IServiceCategoryRepository : IRepository<ServiceCategory>
{
    Task<IEnumerable<ServiceCategory>> GetAllAsync();
    Task<ServiceCategory?> GetByIdAsync(int id);
    ValueTask<EntityEntry<ServiceCategory>> AddAsync(ServiceCategory serviceCategory);
    void Update(ServiceCategory serviceCategory);
    void Delete(ServiceCategory serviceCategory);
}
