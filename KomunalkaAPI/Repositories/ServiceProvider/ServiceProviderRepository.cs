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
}
