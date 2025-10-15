using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.ServiceCategory;

using Models;
public class ServiceCategoryRepository(DbContext context) : Repository<ServiceCategory>(context), IServiceCategoryRepository
{
    public new async Task<IEnumerable<ServiceCategory>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public new Task<ServiceCategory?> GetByIdAsync(int id)
    {
        return _dbSet.FirstOrDefaultAsync(serviceCategory => serviceCategory.Id == id);
    }

    public new ValueTask<EntityEntry<ServiceCategory>> AddAsync(ServiceCategory serviceCategory)
    {
        return _dbSet.AddAsync(serviceCategory);
    }

    public new void Update(ServiceCategory serviceCategory)
    {
        _dbSet.Update(serviceCategory);
    }

    public new void Delete(ServiceCategory serviceCategory)
    {
        _dbSet.Remove(serviceCategory);
    }
}
