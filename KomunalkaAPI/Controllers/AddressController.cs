using AutoMapper;
using KomunalkaAPI.Models.Pagination;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using KomunalkaAPI.Models;
using KomunalkaAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize]
public class AddressController(IUnitOfWork unitOfWork, IMapper mapper) : ControllerBase
{
    [HttpGet(Name = "addresses")]
    public async Task<ActionResult<PagedResult<AddressDto>>> GetAll([FromQuery] PaginationParams paginationParams)
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Невійсний токен користувача");
        }

        // Отримуємо всі адреси з пагінацією
        var pagedAddresses = await unitOfWork.Addresses.GetPagedAsync(paginationParams);

        // Фільтруємо адреси лише для поточного користувача
        var userAddresses = pagedAddresses.Items.Where(a => a.UserId == userId).ToList();
        var addressDtos = mapper.Map<List<AddressDto>>(userAddresses);

        var result = new PagedResult<AddressDto>
        {
            Items = addressDtos,
            PageNumber = pagedAddresses.PageNumber,
            PageSize = pagedAddresses.PageSize,
            TotalCount = pagedAddresses.Items.Count(a => a.UserId == userId) // Кількість адрес користувача
        };

        return Ok(result);
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

        var addressDto = mapper.Map<AddressDto>(address);

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

        var address = mapper.Map<Address>(createAddressDto);
        address.UserId = userId;
        address.User = null!; // Буде заповнено EF
        address.Region = region;
        address.AddressType = addressType;

        var entityEntry = await unitOfWork.Addresses.AddAsync(address);
        await unitOfWork.CompleteAsync();

        var createdAddress = entityEntry.Entity;
        var createdAddressDto = mapper.Map<AddressDto>(createdAddress);

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
        mapper.Map(addressDto, address);
        address.UserId = userId; // Гарантуємо, що UserId не змінюється
        address.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Addresses.Update(address);
        await unitOfWork.CompleteAsync();

        var updatedAddressDto = mapper.Map<AddressDto>(address);

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
