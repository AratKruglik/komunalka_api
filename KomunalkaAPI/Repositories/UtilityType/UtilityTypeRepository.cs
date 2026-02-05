using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.UtilityType;

public class UtilityTypeRepository(DbContext context) : Repository<Models.UtilityType>(context), IUtilityTypeRepository
{
    public async Task<IEnumerable<Models.UtilityType>> GetActiveAsync()
    {
        return await _dbSet.Where(ut => ut.IsActive).ToListAsync();
    }
}
