using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.ServiceCategory;

using Models;

public interface IServiceCategoryRepository
{
    public new Task<IEnumerable<ServiceCategory>> GetAllAsync();
    public new Task<ServiceCategory?> GetByIdAsync(int id);
    public new ValueTask<EntityEntry<ServiceCategory>> AddAsync(ServiceCategory serviceCategory);
    public new void Update(ServiceCategory serviceCategory);
    public new void Delete(ServiceCategory serviceCategory);
}
