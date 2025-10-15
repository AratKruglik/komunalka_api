using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.AddressType;

public interface IAddressTypeService
{
    Task<IReadOnlyList<AddressTypeDto>> GetAllAsync();
    Task<ServiceResult<AddressTypeDto>> GetByIdAsync(int id);
}