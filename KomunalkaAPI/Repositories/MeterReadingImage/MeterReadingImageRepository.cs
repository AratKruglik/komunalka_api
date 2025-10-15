using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.MeterReadingImage;

public class MeterReadingImageRepository : Repository<Models.MeterReadingImage>, IMeterReadingImageRepository
{
    public MeterReadingImageRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.MeterReadingImage>> GetByServiceCounterValueIdAsync(int serviceCounterValueId)
    {
        return await _dbSet
            .Where(img => img.ServiceCounterValueId == serviceCounterValueId)
            .OrderByDescending(img => img.CreatedAt)
            .ToListAsync();
    }

    public async Task<Models.MeterReadingImage?> GetWithServiceCounterValueAsync(int id)
    {
        return await _dbSet
            .Include(img => img.ServiceCounterValue)
            .FirstOrDefaultAsync(img => img.Id == id);
    }
}
