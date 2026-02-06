namespace KomunalkaAPI.Repositories.Tariff;

public interface ITariffRepository : IRepository<Models.Tariff>
{
    Task<IEnumerable<Models.Tariff>> GetByServiceProviderIdAsync(int serviceProviderId);
    Task<Models.Tariff?> GetByIdWithDetailsAsync(int id);
}
