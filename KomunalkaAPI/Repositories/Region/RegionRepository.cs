using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Region;

public class RegionRepository(DbContext context) : Repository<Models.Region>(context), IRegionRepository
{
    public new async Task<IEnumerable<Models.Region>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public new Task<Models.Region?> GetByIdAsync(int id)
    {
        return _dbSet.FirstOrDefaultAsync(x => x.Id == id);
    }

    public new async Task<EntityEntry<Models.Region>> AddAsync(Models.Region region)
    {
        return await _dbSet.AddAsync(region);
    }

    public new void Update(Models.Region region)
    {
        _dbSet.Update(region);
    }

    public new void Delete(Models.Region region)
    {
        _dbSet.Remove(region);
    }
}
