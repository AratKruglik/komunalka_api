using KomunalkaAPI.Repositories.Address;
using KomunalkaAPI.Repositories.Currency;
using KomunalkaAPI.Repositories.ServiceCategory;
using KomunalkaAPI.Repositories.User;
using KomunalkaAPI.Repositories.Region;
using KomunalkaAPI.Repositories.AddressType;

namespace KomunalkaAPI.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IAddressRepository Addresses { get; }
    IServiceCategoryRepository ServiceCategories { get; }
    ICurrencyRepository Currencies { get; }
    IRegionRepository Regions { get; }
    IAddressTypeRepository AddressTypes { get; }
    Task<int> CompleteAsync();
}
