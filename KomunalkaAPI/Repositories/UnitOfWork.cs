using KomunalkaAPI.Repositories.User;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories;

public class UnitOfWork(DbContext context) : IUnitOfWork
{
    public IUserRepository Users { get; } = new UserRepository(context);

    public async Task<int> CompleteAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}
