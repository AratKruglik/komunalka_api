using KomunalkaAPI.Repositories.User;

namespace KomunalkaAPI.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    Task<int> CompleteAsync();
}
