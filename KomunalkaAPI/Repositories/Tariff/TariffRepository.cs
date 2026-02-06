using KomunalkaAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.Tariff;

public class TariffRepository : Repository<Models.Tariff>, ITariffRepository
{
    private readonly ApplicationDbContext _appContext;

    public TariffRepository(ApplicationDbContext context) : base(context)
    {
        _appContext = context;
    }

    public async Task<IEnumerable<Models.Tariff>> GetByServiceProviderIdAsync(int serviceProviderId)
    {
        return await _appContext.Tariffs
            .Where(t => t.ServiceProviderId == serviceProviderId)
            .Include(t => t.UtilityType)
            .Include(t => t.Currency)
            .ToListAsync();
    }

    public async Task<Models.Tariff?> GetByIdWithDetailsAsync(int id)
    {
        return await _appContext.Tariffs
            .Include(t => t.UtilityType)
            .Include(t => t.Currency)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
}
