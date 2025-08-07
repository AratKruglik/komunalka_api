using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using KomunalkaAPI.Models;
using KomunalkaAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet(Name = "addresses")]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetAll()
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Невійсний токен користувача");
        }

        var addresses = await unitOfWork.Addresses.GetAllAsync();
        // Фільтруємо адреси лише для поточного користувача
        IEnumerable<Address> addressList = addresses.Where(a => a.UserId == userId).ToList();

        if (!addressList.Any())
        {
            return NotFound("У вас ще немає збережених адрес");
        }
        
        await unitOfWork.CompleteAsync();

        var addressDtos = addressList.Select(address => new AddressDto
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
            User = address.User,
            Region = address.Region,
            AddressType = address.AddressType,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        }).ToList();

        return Ok(addressDtos);
    }

    [HttpGet("{id:int}", Name = "address")]
    public async Task<ActionResult<AddressDto>> GetById(int id)
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Невійсний токен користувача");
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound("Адресу не знайдено");
        }

        // Перевіряємо, чи адреса належить поточному користувачу
        if (address.UserId != userId)
        {
            return Forbid("У вас немає доступу до цієї адреси");
        }

        await unitOfWork.CompleteAsync();

        var addressDto = new AddressDto
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
            User = address.User,
            Region = address.Region,
            AddressType = address.AddressType,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };

        return Ok(addressDto);
    }

    [HttpPost(Name = "createAddress")]
    public async Task<ActionResult<AddressDto>> Create(CreateAddressDto createAddressDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Невійсний токен користувача");
        }

        // Перевіряємо чи існують Region та AddressType
        var region = await unitOfWork.Regions.GetByIdAsync(createAddressDto.RegionId);
        if (region == null)
        {
            return BadRequest("Вказана область не існує");
        }

        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(createAddressDto.AddressTypeId);
        if (addressType == null)
        {
            return BadRequest("Вказаний тип адреси не існує");
        }

        // Якщо це основна адреса, встановлюємо всі інші адреси користувача як не основні
        if (createAddressDto.IsPrimary)
        {
            var userAddresses = await unitOfWork.Addresses.GetAllAsync();
            var currentUserAddresses = userAddresses.Where(a => a.UserId == userId && a.IsPrimary);
            foreach (var addr in currentUserAddresses)
            {
                addr.IsPrimary = false;
                unitOfWork.Addresses.Update(addr);
            }
        }

        var address = new Address
        {
            UserId = userId,
            RegionId = createAddressDto.RegionId,
            City = createAddressDto.City,
            Street = createAddressDto.Street,
            BuildingNumber = createAddressDto.BuildingNumber,
            ApartmentNumber = createAddressDto.ApartmentNumber,
            ZipCode = createAddressDto.ZipCode,
            Notes = createAddressDto.Notes,
            IsPrimary = createAddressDto.IsPrimary,
            AddressTypeId = createAddressDto.AddressTypeId,
            User = null!, // Буде заповнено EF
            Region = region,
            AddressType = addressType
        };

        var entityEntry = await unitOfWork.Addresses.AddAsync(address);
        await unitOfWork.CompleteAsync();

        var createdAddress = entityEntry.Entity;
        var createdAddressDto = new AddressDto
        {
            Id = createdAddress.Id,
            UserId = createdAddress.UserId,
            RegionId = createdAddress.RegionId,
            City = createdAddress.City,
            Street = createdAddress.Street,
            BuildingNumber = createdAddress.BuildingNumber,
            ApartmentNumber = createdAddress.ApartmentNumber,
            ZipCode = createdAddress.ZipCode,
            Notes = createdAddress.Notes,
            IsPrimary = createdAddress.IsPrimary,
            AddressTypeId = createdAddress.AddressTypeId,
            Region = createdAddress.Region,
            AddressType = createdAddress.AddressType,
            CreatedAt = createdAddress.CreatedAt,
            UpdatedAt = createdAddress.UpdatedAt
        };

        return CreatedAtRoute("address", new { id = createdAddressDto.Id }, createdAddressDto);
    }

    [HttpPut("{id:int}", Name = "updateAddress")]
    public async Task<ActionResult<AddressDto>> Update(int id, AddressDto addressDto)
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Невійсний токен користувача");
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound("Адресу не знайдено");
        }

        // Перевіряємо, чи адреса належить поточному користувачу
        if (address.UserId != userId)
        {
            return Forbid("У вас немає доступу до цієї адреси");
        }

        // Не дозволяємо змінювати UserId - адреса завжди належить поточному користувачу
        address.RegionId = addressDto.RegionId;
        address.ZipCode = addressDto.ZipCode;
        address.City = addressDto.City;
        address.Street = addressDto.Street;
        address.BuildingNumber = addressDto.BuildingNumber;
        address.ApartmentNumber = addressDto.ApartmentNumber;
        address.Notes = addressDto.Notes;
        address.IsPrimary = addressDto.IsPrimary;
        address.AddressTypeId = addressDto.AddressTypeId;
        address.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Addresses.Update(address);
        await unitOfWork.CompleteAsync();

        var updatedAddressDto = new AddressDto
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
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt,
        };

        return Ok(updatedAddressDto);
    }

    [HttpDelete("{id:int}", Name = "deleteAddress")]
    public async Task<ActionResult> Delete(int id)
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Невійсний токен користувача");
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound("Адресу не знайдено");
        }

        // Перевіряємо, чи адреса належить поточному користувачу
        if (address.UserId != userId)
        {
            return Forbid("У вас немає доступу до цієї адреси");
        }

        unitOfWork.Addresses.Delete(address);
        await unitOfWork.CompleteAsync();

        return NoContent();
    }
}
