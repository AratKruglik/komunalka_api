using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.ServiceCategory;

using Models;

public interface IServiceCategoryRepository : IRepository<ServiceCategory>
{
    new Task<IEnumerable<ServiceCategory>> GetAllAsync();
    new Task<ServiceCategory?> GetByIdAsync(int id);
    new ValueTask<EntityEntry<ServiceCategory>> AddAsync(ServiceCategory serviceCategory);
    new void Update(ServiceCategory serviceCategory);
    new void Delete(ServiceCategory serviceCategory);
}
