using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KomunalkaAPI.Repositories.UserAddress;

using Models;

public interface IUserAddressRepository : IRepository<UserAddress>
{
    new Task<IEnumerable<UserAddress>> GetAllAsync();
    new Task<UserAddress?> GetByIdAsync(int id);
    new Task<EntityEntry<UserAddress>> AddAsync(UserAddress userAddress);
    new void Update(UserAddress userAddress);
    new void Delete(UserAddress userAddress);

    /// <summary>
    /// Отримати всі зв'язки адрес для конкретного користувача
    /// </summary>
    Task<List<UserAddress>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримати зв'язок між користувачем та адресою
    /// </summary>
    Task<UserAddress?> GetByUserAndAddressAsync(int userId, int addressId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Отримати основну адресу користувача
    /// </summary>
    Task<UserAddress?> GetPrimaryByUserIdAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Скинути прапорець IsPrimary для всіх адрес користувача
    /// </summary>
    Task ResetPrimaryForUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Перевірити, чи має користувач доступ до адреси
    /// </summary>
    Task<bool> UserHasAccessToAddressAsync(int userId, int addressId, CancellationToken cancellationToken = default);
}
