using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.Address;

public interface IAddressService
{
    Task<(IReadOnlyList<AddressDto> Items, int TotalCount, long LastUpdatedFileTimeUtc)> GetForUserAsync(
        int userId,
        int skip,
        int take,
        string? sortBy,
        bool desc,
        CancellationToken cancellationToken);

    Task<ServiceResult<AddressDto>> GetByIdForUserAsync(int userId, int addressId, CancellationToken cancellationToken);

    Task<ServiceResult<AddressDto>> CreateAsync(int userId, CreateAddressDto dto, CancellationToken cancellationToken);

    Task<ServiceResult<AddressDto>> UpdateAsync(int userId, int addressId, UpdateAddressDto dto, CancellationToken cancellationToken);

    Task<ServiceResult<bool>> DeleteAsync(int userId, int addressId, CancellationToken cancellationToken);
}
