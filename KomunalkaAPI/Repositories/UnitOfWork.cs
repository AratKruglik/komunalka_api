using KomunalkaAPI.Repositories.Address;
using KomunalkaAPI.Repositories.Currency;
using KomunalkaAPI.Repositories.ServiceCategory;
using KomunalkaAPI.Repositories.User;
using KomunalkaAPI.Repositories.Region;
using KomunalkaAPI.Repositories.AddressType;
using KomunalkaAPI.Repositories.RefreshToken;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories;

public class UnitOfWork(DbContext context) : IUnitOfWork
{
    public IUserRepository Users { get; } = new UserRepository(context);
    public IAddressRepository Addresses { get; } = new AddressRepository(context);
    public IServiceCategoryRepository ServiceCategories { get; } = new ServiceCategoryRepository(context);
    public ICurrencyRepository Currencies { get; } = new CurrencyRepository(context);
    public IRegionRepository Regions { get; } = new RegionRepository(context);
    public IAddressTypeRepository AddressTypes { get; } = new AddressTypeRepository(context);
    public IRefreshTokenRepository RefreshTokens { get; } = new RefreshTokenRepository(context);

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
