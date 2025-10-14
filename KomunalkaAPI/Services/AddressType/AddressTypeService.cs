using KomunalkaAPI.DTO;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.AddressType;

public class AddressTypeService(IUnitOfWork unitOfWork) : IAddressTypeService
{
    public async Task<IReadOnlyList<AddressTypeDto>> GetAllAsync()
    {
        var types = await unitOfWork.AddressTypes.GetAllAsync();
        var list = types.ToList();
        var dtos = list.Select(t => new AddressTypeDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description,
            Icon = t.Icon,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt
        }).ToList();
        return dtos;
    }

    public async Task<ServiceResult<AddressTypeDto>> GetByIdAsync(int id)
    {
        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(id);
        if (addressType == null)
        {
            return ServiceResult<AddressTypeDto>.NotFoundResult("Тип адреси не знайдено");
        }

        var dto = new AddressTypeDto
        {
            Id = addressType.Id,
            Name = addressType.Name,
            Description = addressType.Description,
            Icon = addressType.Icon,
            CreatedAt = addressType.CreatedAt,
            UpdatedAt = addressType.UpdatedAt
        };
        return ServiceResult<AddressTypeDto>.Ok(dto);
    }
}