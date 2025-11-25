namespace KomunalkaAPI.Repositories.ServiceProvider;

public interface IServiceProviderRepository : IRepository<Models.ServiceProvider>
{
    Task<IEnumerable<Models.ServiceProvider>> GetActiveProvidersAsync();
    Task<Models.ServiceProvider?> GetByNameAsync(string name);
}
