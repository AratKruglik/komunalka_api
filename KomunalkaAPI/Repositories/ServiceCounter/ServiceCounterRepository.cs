using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.ServiceCounter;

public class ServiceCounterRepository : Repository<Models.ServiceCounter>, IServiceCounterRepository
{
    public ServiceCounterRepository(DbContext context) : base(context)
    {
    }
}
