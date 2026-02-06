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

public class UnitOfWork(Data.ApplicationDbContext context) : IUnitOfWork
{
    public IUserRepository Users { get; } = new UserRepository(context);
    public IAddressRepository Addresses { get; } = new AddressRepository(context);
    public IUserAddressRepository UserAddresses { get; } = new UserAddressRepository(context);
    public IServiceCategoryRepository ServiceCategories { get; } = new ServiceCategoryRepository(context);
    public ICurrencyRepository Currencies { get; } = new CurrencyRepository(context);
    public IRegionRepository Regions { get; } = new RegionRepository(context);
    public IAddressTypeRepository AddressTypes { get; } = new AddressTypeRepository(context);
    public IRefreshTokenRepository RefreshTokens { get; } = new RefreshTokenRepository(context);
    public IMeterReadingImageRepository MeterReadingImages { get; } = new MeterReadingImageRepository(context);
    public IServiceCounterRepository ServiceCounters { get; } = new ServiceCounterRepository(context);
    public IServiceCounterValueRepository ServiceCounterValues { get; } = new ServiceCounterValueRepository(context);
    public IMeterRepository Meters { get; } = new MeterRepository(context);
    public IServiceProviderRepository ServiceProviders { get; } = new ServiceProviderRepository(context);
    public IMeterReadingRepository MeterReadings { get; } = new MeterReadingRepository(context);
    public IUtilityTypeRepository UtilityTypes { get; } = new UtilityTypeRepository(context);
    public ITariffRepository Tariffs { get; } = new TariffRepository(context);

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public DbContext GetContext()
    {
        return context;
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
