using KomunalkaAPI.Repositories.Address;
using KomunalkaAPI.Repositories.ServiceCategory;
using KomunalkaAPI.Repositories.User;

namespace KomunalkaAPI.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IAddressRepository Addresses { get; }
    IServiceCategoryRepository ServiceCategories { get; }
    Task<int> CompleteAsync();
}
