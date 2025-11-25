using KomunalkaAPI.Models;

namespace KomunalkaAPI.Repositories.Meter;

public interface IMeterRepository : IRepository<Models.Meter>
{
    Task<IEnumerable<Models.Meter>> GetByAddressIdAsync(int addressId);
    Task<IEnumerable<Models.Meter>> GetByUtilityTypeIdAsync(int utilityTypeId);
    Task<IEnumerable<Models.Meter>> GetByServiceProviderIdAsync(int serviceProviderId);
    Task<Models.Meter?> GetBySerialNumberAsync(string serialNumber);
    Task<IEnumerable<Models.Meter>> GetActiveMetersAsync();
}
