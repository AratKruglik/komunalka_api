using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.Address;

public class AddressService(IUnitOfWork unitOfWork) : IAddressService
{
    public async Task<(IReadOnlyList<AddressDto> Items, int TotalCount, long LastUpdatedFileTimeUtc)> GetForUserAsync(
        int userId,
        int skip,
        int take,
        string? sortBy,
        bool desc,
        CancellationToken cancellationToken)
    {
        var addressList = await unitOfWork.Addresses.GetByUserIdAsync(userId, skip, take, sortBy, desc, includeDeps: true, cancellationToken);
        var totalCount = await unitOfWork.Addresses.CountByUserIdAsync(userId, cancellationToken);

        var lastUpdatedTicks = addressList.Count > 0 ? addressList.Max(a => a.UpdatedAt).ToFileTimeUtc() : 0;

        var dtos = addressList.Select(address => new AddressDto
        {
            Id = address.Id,
            UserId = address.UserId,
            RegionId = address.RegionId,
            ZipCode = address.ZipCode,
            City = address.City,
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            ApartmentNumber = address.ApartmentNumber,
            Notes = address.Notes,
            IsPrimary = address.IsPrimary,
            AddressTypeId = address.AddressTypeId,
            Region = address.Region != null ? new RegionDto
            {
                Id = address.Region.Id,
                Name = address.Region.Name,
                CreatedAt = address.Region.CreatedAt,
                UpdatedAt = address.Region.UpdatedAt
            } : null,
            AddressType = address.AddressType != null ? new AddressTypeDto
            {
                Id = address.AddressType.Id,
                Name = address.AddressType.Name,
                Description = address.AddressType.Description,
                Icon = address.AddressType.Icon,
                CreatedAt = address.AddressType.CreatedAt,
                UpdatedAt = address.AddressType.UpdatedAt
            } : null,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        }).ToList();

        return (dtos, totalCount, lastUpdatedTicks);
    }

    public async Task<ServiceResult<AddressDto>> GetByIdForUserAsync(int userId, int addressId, CancellationToken cancellationToken)
    {
        var address = await unitOfWork.Addresses.GetByIdAsync(addressId);
        if (address == null)
        {
            return ServiceResult<AddressDto>.NotFoundResult("Адресу не знайдено");
        }

        if (address.UserId != userId)
        {
            return ServiceResult<AddressDto>.ForbiddenResult();
        }

        var dto = new AddressDto
        {
            Id = address.Id,
            UserId = address.UserId,
            RegionId = address.RegionId,
            ZipCode = address.ZipCode,
            City = address.City,
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            ApartmentNumber = address.ApartmentNumber,
            Notes = address.Notes,
            IsPrimary = address.IsPrimary,
            AddressTypeId = address.AddressTypeId,
            Region = address.Region != null ? new RegionDto
            {
                Id = address.Region.Id,
                Name = address.Region.Name,
                CreatedAt = address.Region.CreatedAt,
                UpdatedAt = address.Region.UpdatedAt
            } : null,
            AddressType = address.AddressType != null ? new AddressTypeDto
            {
                Id = address.AddressType.Id,
                Name = address.AddressType.Name,
                Description = address.AddressType.Description,
                Icon = address.AddressType.Icon,
                CreatedAt = address.AddressType.CreatedAt,
                UpdatedAt = address.AddressType.UpdatedAt
            } : null,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };

        return ServiceResult<AddressDto>.Ok(dto);
    }

    public async Task<ServiceResult<AddressDto>> CreateAsync(int userId, CreateAddressDto dto, CancellationToken cancellationToken)
    {
        var region = await unitOfWork.Regions.GetByIdAsync(dto.RegionId);
        if (region == null)
        {
            return ServiceResult<AddressDto>.Fail("Вказана область не існує");
        }

        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(dto.AddressTypeId);
        if (addressType == null)
        {
            return ServiceResult<AddressDto>.Fail("Вказаний тип адреси не існує");
        }

        if (dto.IsPrimary)
        {
            var currentUserAddresses = await unitOfWork.Addresses.GetUserAddressesAsync(userId, cancellationToken);
            foreach (var addr in currentUserAddresses.Where(a => a.IsPrimary))
            {
                addr.IsPrimary = false;
                unitOfWork.Addresses.Update(addr);
            }
        }

        var address = new Models.Address
        {
            UserId = userId,
            RegionId = dto.RegionId,
            City = dto.City,
            Street = dto.Street,
            BuildingNumber = dto.BuildingNumber,
            ApartmentNumber = dto.ApartmentNumber,
            ZipCode = dto.ZipCode,
            Notes = dto.Notes,
            IsPrimary = dto.IsPrimary,
            AddressTypeId = dto.AddressTypeId,
            User = null!,
            Region = region,
            AddressType = addressType
        };

        var entityEntry = await unitOfWork.Addresses.AddAsync(address);
        await unitOfWork.CompleteAsync();

        var created = entityEntry.Entity;
        var createdDto = new AddressDto
        {
            Id = created.Id,
            UserId = created.UserId,
            RegionId = created.RegionId,
            City = created.City,
            Street = created.Street,
            BuildingNumber = created.BuildingNumber,
            ApartmentNumber = created.ApartmentNumber,
            ZipCode = created.ZipCode,
            Notes = created.Notes,
            IsPrimary = created.IsPrimary,
            AddressTypeId = created.AddressTypeId,
            Region = created.Region != null ? new RegionDto
            {
                Id = created.Region.Id,
                Name = created.Region.Name,
                CreatedAt = created.Region.CreatedAt,
                UpdatedAt = created.Region.UpdatedAt
            } : null,
            AddressType = created.AddressType != null ? new AddressTypeDto
            {
                Id = created.AddressType.Id,
                Name = created.AddressType.Name,
                Description = created.AddressType.Description,
                Icon = created.AddressType.Icon,
                CreatedAt = created.AddressType.CreatedAt,
                UpdatedAt = created.AddressType.UpdatedAt
            } : null,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };

        return ServiceResult<AddressDto>.Ok(createdDto);
    }

    public async Task<ServiceResult<AddressDto>> UpdateAsync(int userId, int addressId, UpdateAddressDto dto, CancellationToken cancellationToken)
    {
        var address = await unitOfWork.Addresses.GetByIdAsync(addressId);
        if (address == null)
        {
            return ServiceResult<AddressDto>.NotFoundResult("Адресу не знайдено");
        }
        if (address.UserId != userId)
        {
            return ServiceResult<AddressDto>.ForbiddenResult();
        }

        var region = await unitOfWork.Regions.GetByIdAsync(dto.RegionId);
        if (region == null)
        {
            return ServiceResult<AddressDto>.Fail("Вказана область не існує");
        }
        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(dto.AddressTypeId);
        if (addressType == null)
        {
            return ServiceResult<AddressDto>.Fail("Вказаний тип адреси не існує");
        }

        if (dto.IsPrimary)
        {
            var currentUserAddresses = await unitOfWork.Addresses.GetUserAddressesAsync(userId, cancellationToken);
            foreach (var addr in currentUserAddresses.Where(a => a.IsPrimary && a.Id != addressId))
            {
                addr.IsPrimary = false;
                unitOfWork.Addresses.Update(addr);
            }
        }

        address.RegionId = dto.RegionId;
        address.ZipCode = dto.ZipCode;
        address.City = dto.City;
        address.Street = dto.Street;
        address.BuildingNumber = dto.BuildingNumber;
        address.ApartmentNumber = dto.ApartmentNumber;
        address.Notes = dto.Notes;
        address.IsPrimary = dto.IsPrimary;
        address.AddressTypeId = dto.AddressTypeId;
        address.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Addresses.Update(address);
        await unitOfWork.CompleteAsync();

        var updatedDto = new AddressDto
        {
            Id = address.Id,
            UserId = address.UserId,
            RegionId = address.RegionId,
            ZipCode = address.ZipCode,
            City = address.City,
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            ApartmentNumber = address.ApartmentNumber,
            Notes = address.Notes,
            IsPrimary = address.IsPrimary,
            AddressTypeId = address.AddressTypeId,
            Region = region != null ? new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                CreatedAt = region.CreatedAt,
                UpdatedAt = region.UpdatedAt
            } : null,
            AddressType = addressType != null ? new AddressTypeDto
            {
                Id = addressType.Id,
                Name = addressType.Name,
                Description = addressType.Description,
                Icon = addressType.Icon,
                CreatedAt = addressType.CreatedAt,
                UpdatedAt = addressType.UpdatedAt
            } : null,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };

        return ServiceResult<AddressDto>.Ok(updatedDto);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int userId, int addressId, CancellationToken cancellationToken)
    {
        var address = await unitOfWork.Addresses.GetByIdAsync(addressId);
        if (address == null)
        {
            return ServiceResult<bool>.NotFoundResult("Адресу не знайдено");
        }
        if (address.UserId != userId)
        {
            return ServiceResult<bool>.ForbiddenResult();
        }

        unitOfWork.Addresses.Delete(address);
        await unitOfWork.CompleteAsync();
        return ServiceResult<bool>.Ok(true);
    }
}
