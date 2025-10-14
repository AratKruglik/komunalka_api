using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Repositories.ServiceCounterValue;

public class ServiceCounterValueRepository : Repository<Models.ServiceCounterValue>, IServiceCounterValueRepository
{
    public ServiceCounterValueRepository(DbContext context) : base(context)
    {
    }
}
