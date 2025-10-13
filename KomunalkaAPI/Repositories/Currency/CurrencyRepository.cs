using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.Currency;

using Models;

public class CurrencyRepository : Repository<Currency>, ICurrencyRepository
{
    public CurrencyRepository(DbContext context) : base(context)
    {
    }

    public override async Task<IEnumerable<Currency>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public override async Task<Currency?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public override async Task<EntityEntry<Currency>> AddAsync(Currency currency)
    {
        return await _dbSet.AddAsync(currency);
    }

    public override void Update(Currency currency)
    {
        _dbSet.Update(currency);
    }

    public override void Delete(Currency currency)
    {
        _dbSet.Remove(currency);
    }
}
