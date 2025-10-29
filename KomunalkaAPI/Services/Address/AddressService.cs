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
        // Отримуємо UserAddress зв'язки для користувача
        var userAddresses = await unitOfWork.UserAddresses.GetByUserIdAsync(userId, cancellationToken);

        var totalCount = userAddresses.Count;

        // Застосовуємо пагінацію та сортування
        var sorted = sortBy?.ToLowerInvariant() switch
        {
            "city" => desc
                ? userAddresses.OrderByDescending(ua => ua.Address.City)
                : userAddresses.OrderBy(ua => ua.Address.City),
            "updatedat" => desc
                ? userAddresses.OrderByDescending(ua => ua.Address.UpdatedAt)
                : userAddresses.OrderBy(ua => ua.Address.UpdatedAt),
            "isprimary" => desc
                ? userAddresses.OrderByDescending(ua => ua.IsPrimary).ThenByDescending(ua => ua.Address.UpdatedAt)
                : userAddresses.OrderBy(ua => ua.IsPrimary).ThenBy(ua => ua.Address.UpdatedAt),
            _ => desc
                ? userAddresses.OrderByDescending(ua => ua.Address.CreatedAt)
                : userAddresses.OrderBy(ua => ua.Address.CreatedAt)
        };

        var paged = sorted.Skip(skip).Take(take > 0 ? take : int.MaxValue).ToList();

        var lastUpdatedTicks = paged.Count > 0 ? paged.Max(ua => ua.Address.UpdatedAt).ToFileTimeUtc() : 0;

        var dtos = paged.Select(ua => new AddressDto
        {
            Id = ua.Address.Id,
            UserId = ua.UserId, // З UserAddress
            RegionId = ua.Address.RegionId,
            ZipCode = ua.Address.ZipCode,
            City = ua.Address.City,
            Street = ua.Address.Street,
            BuildingNumber = ua.Address.BuildingNumber,
            ApartmentNumber = ua.Address.ApartmentNumber,
            Notes = ua.Address.Notes,
            IsPrimary = ua.IsPrimary, // З UserAddress
            AddressTypeId = ua.Address.AddressTypeId,
            Region = ua.Address.Region != null ? new RegionDto
            {
                Id = ua.Address.Region.Id,
                Name = ua.Address.Region.Name,
                CreatedAt = ua.Address.Region.CreatedAt,
                UpdatedAt = ua.Address.Region.UpdatedAt
            } : null,
            AddressType = ua.Address.AddressType != null ? new AddressTypeDto
            {
                Id = ua.Address.AddressType.Id,
                Name = ua.Address.AddressType.Name,
                Description = ua.Address.AddressType.Description,
                Icon = ua.Address.AddressType.Icon,
                CreatedAt = ua.Address.AddressType.CreatedAt,
                UpdatedAt = ua.Address.AddressType.UpdatedAt
            } : null,
            CreatedAt = ua.Address.CreatedAt,
            UpdatedAt = ua.Address.UpdatedAt
        }).ToList();

        return (dtos, totalCount, lastUpdatedTicks);
    }

    public async Task<ServiceResult<AddressDto>> GetByIdForUserAsync(int userId, int addressId, CancellationToken cancellationToken)
    {
        // Перевіряємо, чи має користувач доступ до цієї адреси
        var userAddress = await unitOfWork.UserAddresses.GetByUserAndAddressAsync(userId, addressId, cancellationToken);
        if (userAddress == null)
        {
            return ServiceResult<AddressDto>.NotFoundResult("Адресу не знайдено або у вас немає доступу до неї");
        }

        var dto = new AddressDto
        {
            Id = userAddress.Address.Id,
            UserId = userAddress.UserId,
            RegionId = userAddress.Address.RegionId,
            ZipCode = userAddress.Address.ZipCode,
            City = userAddress.Address.City,
            Street = userAddress.Address.Street,
            BuildingNumber = userAddress.Address.BuildingNumber,
            ApartmentNumber = userAddress.Address.ApartmentNumber,
            Notes = userAddress.Address.Notes,
            IsPrimary = userAddress.IsPrimary,
            AddressTypeId = userAddress.Address.AddressTypeId,
            Region = userAddress.Address.Region != null ? new RegionDto
            {
                Id = userAddress.Address.Region.Id,
                Name = userAddress.Address.Region.Name,
                CreatedAt = userAddress.Address.Region.CreatedAt,
                UpdatedAt = userAddress.Address.Region.UpdatedAt
            } : null,
            AddressType = userAddress.Address.AddressType != null ? new AddressTypeDto
            {
                Id = userAddress.Address.AddressType.Id,
                Name = userAddress.Address.AddressType.Name,
                Description = userAddress.Address.AddressType.Description,
                Icon = userAddress.Address.AddressType.Icon,
                CreatedAt = userAddress.Address.AddressType.CreatedAt,
                UpdatedAt = userAddress.Address.AddressType.UpdatedAt
            } : null,
            CreatedAt = userAddress.Address.CreatedAt,
            UpdatedAt = userAddress.Address.UpdatedAt
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

        // Якщо адреса має бути основною, скидаємо прапорець у інших адрес користувача
        if (dto.IsPrimary)
        {
            await unitOfWork.UserAddresses.ResetPrimaryForUserAsync(userId, cancellationToken);
        }

        // Створюємо адресу
        var address = new Models.Address
        {
            RegionId = dto.RegionId,
            City = dto.City,
            Street = dto.Street,
            BuildingNumber = dto.BuildingNumber,
            ApartmentNumber = dto.ApartmentNumber,
            ZipCode = dto.ZipCode,
            Notes = dto.Notes,
            AddressTypeId = dto.AddressTypeId,
            Region = region,
            AddressType = addressType
        };

        var addressEntry = await unitOfWork.Addresses.AddAsync(address);
        await unitOfWork.CompleteAsync(); // Зберігаємо адресу, щоб отримати Id

        // Створюємо зв'язок User-Address
        var userAddress = new UserAddress
        {
            UserId = userId,
            AddressId = addressEntry.Entity.Id,
            IsPrimary = dto.IsPrimary,
            User = null!,
            Address = addressEntry.Entity
        };

        await unitOfWork.UserAddresses.AddAsync(userAddress);
        await unitOfWork.CompleteAsync();

        var createdDto = new AddressDto
        {
            Id = addressEntry.Entity.Id,
            UserId = userId,
            RegionId = addressEntry.Entity.RegionId,
            City = addressEntry.Entity.City,
            Street = addressEntry.Entity.Street,
            BuildingNumber = addressEntry.Entity.BuildingNumber,
            ApartmentNumber = addressEntry.Entity.ApartmentNumber,
            ZipCode = addressEntry.Entity.ZipCode,
            Notes = addressEntry.Entity.Notes,
            IsPrimary = dto.IsPrimary,
            AddressTypeId = addressEntry.Entity.AddressTypeId,
            Region = new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                CreatedAt = region.CreatedAt,
                UpdatedAt = region.UpdatedAt
            },
            AddressType = new AddressTypeDto
            {
                Id = addressType.Id,
                Name = addressType.Name,
                Description = addressType.Description,
                Icon = addressType.Icon,
                CreatedAt = addressType.CreatedAt,
                UpdatedAt = addressType.UpdatedAt
            },
            CreatedAt = addressEntry.Entity.CreatedAt,
            UpdatedAt = addressEntry.Entity.UpdatedAt
        };

        return ServiceResult<AddressDto>.Ok(createdDto);
    }

    public async Task<ServiceResult<AddressDto>> UpdateAsync(int userId, int addressId, UpdateAddressDto dto, CancellationToken cancellationToken)
    {
        // Перевіряємо доступ користувача до адреси
        var userAddress = await unitOfWork.UserAddresses.GetByUserAndAddressAsync(userId, addressId, cancellationToken);
        if (userAddress == null)
        {
            return ServiceResult<AddressDto>.NotFoundResult("Адресу не знайдено або у вас немає доступу до неї");
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

        // Якщо адреса має стати основною, скидаємо прапорець у інших
        if (dto.IsPrimary && !userAddress.IsPrimary)
        {
            await unitOfWork.UserAddresses.ResetPrimaryForUserAsync(userId, cancellationToken);
        }

        // Оновлюємо адресу
        var address = userAddress.Address;
        address.RegionId = dto.RegionId;
        address.ZipCode = dto.ZipCode;
        address.City = dto.City;
        address.Street = dto.Street;
        address.BuildingNumber = dto.BuildingNumber;
        address.ApartmentNumber = dto.ApartmentNumber;
        address.Notes = dto.Notes;
        address.AddressTypeId = dto.AddressTypeId;
        address.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Addresses.Update(address);

        // Оновлюємо UserAddress (IsPrimary)
        userAddress.IsPrimary = dto.IsPrimary;
        userAddress.UpdatedAt = DateTime.UtcNow;
        unitOfWork.UserAddresses.Update(userAddress);

        await unitOfWork.CompleteAsync();

        var updatedDto = new AddressDto
        {
            Id = address.Id,
            UserId = userId,
            RegionId = address.RegionId,
            ZipCode = address.ZipCode,
            City = address.City,
            Street = address.Street,
            BuildingNumber = address.BuildingNumber,
            ApartmentNumber = address.ApartmentNumber,
            Notes = address.Notes,
            IsPrimary = userAddress.IsPrimary,
            AddressTypeId = address.AddressTypeId,
            Region = new RegionDto
            {
                Id = region.Id,
                Name = region.Name,
                CreatedAt = region.CreatedAt,
                UpdatedAt = region.UpdatedAt
            },
            AddressType = new AddressTypeDto
            {
                Id = addressType.Id,
                Name = addressType.Name,
                Description = addressType.Description,
                Icon = addressType.Icon,
                CreatedAt = addressType.CreatedAt,
                UpdatedAt = addressType.UpdatedAt
            },
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };

        return ServiceResult<AddressDto>.Ok(updatedDto);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int userId, int addressId, CancellationToken cancellationToken)
    {
        // Перевіряємо доступ
        var userAddress = await unitOfWork.UserAddresses.GetByUserAndAddressAsync(userId, addressId, cancellationToken);
        if (userAddress == null)
        {
            return ServiceResult<bool>.NotFoundResult("Адресу не знайдено або у вас немає доступу до неї");
        }

        // Видаляємо зв'язок (не саму адресу, бо вона може бути спільною)
        unitOfWork.UserAddresses.Delete(userAddress);

        // Перевіряємо, чи є ще користувачі, які використовують цю адресу
        var address = await unitOfWork.Addresses.GetByIdAsync(addressId);
        if (address?.UserAddresses != null && address.UserAddresses.Count <= 1)
        {
            // Якщо це був останній користувач, видаляємо адресу (soft delete)
            address.DeletedAt = DateTime.UtcNow;
            unitOfWork.Addresses.Update(address);
        }

        await unitOfWork.CompleteAsync();
        return ServiceResult<bool>.Ok(true);
    }
}
