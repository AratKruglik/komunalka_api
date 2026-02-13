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
            .Include(mr => mr.Tariff)
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
            .Include(mr => mr.Tariff)
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
            .Include(mr => mr.Tariff)
            .OrderByDescending(mr => mr.ReadingDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Models.MeterReading?> GetLatestByMeterAndTariffAsync(int meterId, int tariffId)
    {
        return await _dbSet
            .Where(mr => mr.MeterId == meterId && mr.TariffId == tariffId)
            .Include(mr => mr.Meter)
                .ThenInclude(m => m.UtilityType)
            .Include(mr => mr.Photos)
            .Include(mr => mr.Tariff)
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
            .Include(mr => mr.Tariff)
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
            .Include(mr => mr.Tariff)
            .OrderByDescending(mr => mr.ReadingDate)
            .ToListAsync();
    }

}
