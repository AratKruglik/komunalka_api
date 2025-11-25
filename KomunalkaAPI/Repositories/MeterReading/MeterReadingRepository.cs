using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.MeterReading;

public class MeterReadingRepository : Repository<Models.MeterReading>, IMeterReadingRepository
{
    public MeterReadingRepository(DbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.MeterReading>> GetByMeterIdAsync(int meterId)
    {
        return await _dbSet
            .Where(mr => mr.MeterId == meterId)
            .Include(mr => mr.Meter)
                .ThenInclude(m => m.UtilityType)
            .Include(mr => mr.Photos)
            .OrderByDescending(mr => mr.ReadingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.MeterReading>> GetByAddressIdAsync(int addressId)
    {
        return await _dbSet
            .Where(mr => mr.Meter.AddressId == addressId)
            .Include(mr => mr.Meter)
                .ThenInclude(m => m.UtilityType)
            .Include(mr => mr.Photos)
            .OrderByDescending(mr => mr.ReadingDate)
            .ToListAsync();
    }

    public async Task<Models.MeterReading?> GetLatestByMeterIdAsync(int meterId)
    {
        return await _dbSet
            .Where(mr => mr.MeterId == meterId)
            .Include(mr => mr.Meter)
                .ThenInclude(m => m.UtilityType)
            .Include(mr => mr.Photos)
            .OrderByDescending(mr => mr.ReadingDate)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Models.MeterReading>> GetByDateRangeAsync(int meterId, DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(mr => mr.MeterId == meterId && mr.ReadingDate >= from && mr.ReadingDate <= to)
            .Include(mr => mr.Meter)
                .ThenInclude(m => m.UtilityType)
            .Include(mr => mr.Photos)
            .OrderByDescending(mr => mr.ReadingDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.MeterReading>> GetByAddressAndDateRangeAsync(int addressId, DateTime from, DateTime to)
    {
        return await _dbSet
            .Where(mr => mr.Meter.AddressId == addressId && mr.ReadingDate >= from && mr.ReadingDate <= to)
            .Include(mr => mr.Meter)
                .ThenInclude(m => m.UtilityType)
            .Include(mr => mr.Photos)
            .OrderByDescending(mr => mr.ReadingDate)
            .ToListAsync();
    }

    public async Task<bool> HasReadingOnDateAsync(int meterId, DateTime date)
    {
        var dateOnly = date.Date;
        return await _dbSet
            .AnyAsync(mr => mr.MeterId == meterId && mr.ReadingDate.Date == dateOnly);
    }
}
