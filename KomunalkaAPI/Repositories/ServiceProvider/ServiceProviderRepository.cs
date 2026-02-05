using KomunalkaAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.ServiceProvider;

public class ServiceProviderRepository : Repository<Models.ServiceProvider>, IServiceProviderRepository
{
    private readonly ApplicationDbContext _appContext;

    public ServiceProviderRepository(ApplicationDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<IEnumerable<Models.ServiceProvider>> GetActiveProvidersAsync()
    {
        return await _appContext.ServiceProviders
            .Where(sp => sp.IsActive)
            .ToListAsync();
    }

    public async Task<Models.ServiceProvider?> GetByNameAsync(string name)
    {
        return await _appContext.ServiceProviders
            .FirstOrDefaultAsync(sp => sp.Name == name);
    }

    public async Task<IEnumerable<Models.ServiceProvider>> GetByAddressIdAsync(int addressId)
    {
        return await _appContext.ServiceProviders
            .Where(sp => sp.AddressId == addressId)
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.UtilityType)
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.Currency)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.ServiceProvider>> GetByAddressIdsAsync(IEnumerable<int> addressIds)
    {
        var ids = addressIds.ToList();
        return await _appContext.ServiceProviders
            .Where(sp => ids.Contains(sp.AddressId))
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.UtilityType)
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.Currency)
            .ToListAsync();
    }
}
