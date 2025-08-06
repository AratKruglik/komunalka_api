using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Currency;

using Models;

public interface ICurrencyRepository : IRepository<Currency>
{
    Task<IEnumerable<Currency>> GetAllAsync();
    Task<Currency?> GetByIdAsync(int id);
    Task<EntityEntry<Currency>> AddAsync(Currency currency);
    void Update(Currency currency);
    void Delete(Currency currency);
}
