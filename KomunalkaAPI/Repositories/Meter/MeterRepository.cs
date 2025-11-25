using KomunalkaAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.Meter;

public class MeterRepository : Repository<Models.Meter>, IMeterRepository
{
    private readonly ApplicationDbContext _appContext;

    public MeterRepository(ApplicationDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<IEnumerable<Models.Meter>> GetByAddressIdAsync(int addressId)
    {
        return await _appContext.Meters
            .Include(m => m.UtilityType)
            .Include(m => m.ServiceProvider)
            .Where(m => m.AddressId == addressId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Meter>> GetByUtilityTypeIdAsync(int utilityTypeId)
    {
        return await _appContext.Meters
            .Include(m => m.Address)
            .Include(m => m.ServiceProvider)
            .Where(m => m.UtilityTypeId == utilityTypeId)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Meter>> GetByServiceProviderIdAsync(int serviceProviderId)
    {
        return await _appContext.Meters
            .Include(m => m.Address)
            .Include(m => m.UtilityType)
            .Where(m => m.ServiceProviderId == serviceProviderId)
            .ToListAsync();
    }

    public async Task<Models.Meter?> GetBySerialNumberAsync(string serialNumber)
    {
        return await _appContext.Meters
            .Include(m => m.Address)
            .Include(m => m.UtilityType)
            .Include(m => m.ServiceProvider)
            .FirstOrDefaultAsync(m => m.SerialNumber == serialNumber);
    }

    public async Task<IEnumerable<Models.Meter>> GetActiveMetersAsync()
    {
        return await _appContext.Meters
            .Include(m => m.Address)
            .Include(m => m.UtilityType)
            .Include(m => m.ServiceProvider)
            .Where(m => m.IsActive)
            .ToListAsync();
    }
}
