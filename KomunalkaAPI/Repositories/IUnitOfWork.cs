using KomunalkaAPI.Repositories.Address;
using KomunalkaAPI.Repositories.Currency;
using KomunalkaAPI.Repositories.ServiceCategory;
using KomunalkaAPI.Repositories.User;
using KomunalkaAPI.Repositories.Region;
using KomunalkaAPI.Repositories.AddressType;
using KomunalkaAPI.Repositories.RefreshToken;
using KomunalkaAPI.Repositories.MeterReadingImage;
using KomunalkaAPI.Repositories.ServiceCounter;
using KomunalkaAPI.Repositories.ServiceCounterValue;
using KomunalkaAPI.Repositories.UserAddress;
using KomunalkaAPI.Repositories.Meter;
using KomunalkaAPI.Repositories.ServiceProvider;
using KomunalkaAPI.Repositories.MeterReading;
using KomunalkaAPI.Repositories.Tariff;
using KomunalkaAPI.Repositories.UtilityType;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IAddressRepository Addresses { get; }
    IUserAddressRepository UserAddresses { get; }
    IServiceCategoryRepository ServiceCategories { get; }
    ICurrencyRepository Currencies { get; }
    IRegionRepository Regions { get; }
    IAddressTypeRepository AddressTypes { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    IMeterReadingImageRepository MeterReadingImages { get; }
    IServiceCounterRepository ServiceCounters { get; }
    IServiceCounterValueRepository ServiceCounterValues { get; }
    IMeterRepository Meters { get; }
    IServiceProviderRepository ServiceProviders { get; }
    IMeterReadingRepository MeterReadings { get; }
    ITariffRepository Tariffs { get; }
    IUtilityTypeRepository UtilityTypes { get; }
    Task<int> CompleteAsync();
    DbContext GetContext();
}
