namespace KomunalkaAPI.Repositories.ServiceProvider;

public interface IServiceProviderRepository : IRepository<Models.ServiceProvider>
{
    Task<IEnumerable<Models.ServiceProvider>> GetActiveProvidersAsync();
    Task<Models.ServiceProvider?> GetByNameAsync(string name);
    Task<IEnumerable<Models.ServiceProvider>> GetByAddressIdAsync(int addressId);
    Task<IEnumerable<Models.ServiceProvider>> GetByAddressIdsAsync(IEnumerable<int> addressIds);
}
