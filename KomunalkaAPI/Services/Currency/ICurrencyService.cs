using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.Currency;

public interface ICurrencyService
{
    Task<IReadOnlyList<CurrencyDto>> GetAllAsync();
    Task<ServiceResult<CurrencyDto>> GetByIdAsync(int id);
    Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto);
    Task<ServiceResult<CurrencyDto>> UpdateAsync(int id, UpdateCurrencyDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}