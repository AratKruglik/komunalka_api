using KomunalkaAPI.Models;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Region;

public interface IRegionRepository : IRepository<Models.Region>
{
    new Task<IEnumerable<Models.Region>> GetAllAsync();
    new Task<Models.Region?> GetByIdAsync(int id);
    new Task<EntityEntry<Models.Region>> AddAsync(Models.Region region);
    new void Update(Models.Region region);
    new void Delete(Models.Region region);
}
