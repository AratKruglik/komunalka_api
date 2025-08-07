using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Currency;

using Models;

public interface ICurrencyRepository : IRepository<Currency>
{
    new Task<IEnumerable<Currency>> GetAllAsync();
    new Task<Currency?> GetByIdAsync(int id);
    new Task<EntityEntry<Currency>> AddAsync(Currency currency);
    new void Update(Currency currency);
    new void Delete(Currency currency);
}
