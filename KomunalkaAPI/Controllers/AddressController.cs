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
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        CancellationToken cancellationToken = default)
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        // Отримуємо адреси користувача з БД (залежності включені)
        var addressList = await unitOfWork.Addresses.GetByUserIdAsync(userId, skip, take, sortBy, desc, includeDeps: true, cancellationToken);
        var totalCount = await unitOfWork.Addresses.CountByUserIdAsync(userId, cancellationToken);

        if (totalCount == 0)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "У вас ще немає збережених адрес", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }

        // ETag for caching
        var lastUpdatedTicks = addressList.Count > 0 ? addressList.Max(a => a.UpdatedAt).ToFileTimeUtc() : 0;
        var etag = $"W/\"addr-{userId}-{totalCount}-{lastUpdatedTicks}\"";
        var ifNoneMatch = Request.Headers["If-None-Match"].ToString();
        if (!string.IsNullOrEmpty(ifNoneMatch) && string.Equals(ifNoneMatch, etag, StringComparison.Ordinal))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "private, max-age=30";
        Response.Headers["X-Total-Count"] = totalCount.ToString();
        Response.Headers["X-Skip"] = skip.ToString();
        Response.Headers["X-Take"] = take.ToString();

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

        return Ok(addressDtos);
    }

    [HttpGet("{id:int}", Name = "address")]
    public async Task<ActionResult<AddressDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Адресу не знайдено", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }

        // Перевіряємо, чи адреса належить поточному користувачу
        if (address.UserId != userId)
        {
            return Forbid();
        }

        // ETag
        var etag = $"W/\"addr-{address.Id}-{address.UpdatedAt.ToFileTimeUtc()}\"";
        var ifNoneMatch = Request.Headers["If-None-Match"].ToString();
        if (!string.IsNullOrEmpty(ifNoneMatch) && string.Equals(ifNoneMatch, etag, StringComparison.Ordinal))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "private, max-age=60";

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

        return Ok(addressDto);
    }

    [HttpPost(Name = "createAddress")]
    public async Task<ActionResult<AddressDto>> Create(CreateAddressDto createAddressDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        // Перевіряємо чи існують Region та AddressType
        var region = await unitOfWork.Regions.GetByIdAsync(createAddressDto.RegionId);
        if (region == null)
        {
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Вказана область не існує", Status = StatusCodes.Status400BadRequest });
        }

        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(createAddressDto.AddressTypeId);
        if (addressType == null)
        {
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Вказаний тип адреси не існує", Status = StatusCodes.Status400BadRequest });
        }

        // Якщо це основна адреса, встановлюємо всі інші адреси користувача як не основні
        if (createAddressDto.IsPrimary)
        {
            var currentUserAddresses = await unitOfWork.Addresses.GetUserAddressesAsync(userId, cancellationToken);
            foreach (var addr in currentUserAddresses.Where(a => a.IsPrimary))
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
            Region = createdAddress.Region != null ? new RegionDto
            {
                Id = createdAddress.Region.Id,
                Name = createdAddress.Region.Name,
                CreatedAt = createdAddress.Region.CreatedAt,
                UpdatedAt = createdAddress.Region.UpdatedAt
            } : null,
            AddressType = createdAddress.AddressType != null ? new AddressTypeDto
            {
                Id = createdAddress.AddressType.Id,
                Name = createdAddress.AddressType.Name,
                Description = createdAddress.AddressType.Description,
                Icon = createdAddress.AddressType.Icon,
                CreatedAt = createdAddress.AddressType.CreatedAt,
                UpdatedAt = createdAddress.AddressType.UpdatedAt
            } : null,
            CreatedAt = createdAddress.CreatedAt,
            UpdatedAt = createdAddress.UpdatedAt
        };

        return CreatedAtRoute("address", new { id = createdAddressDto.Id }, createdAddressDto);
    }

    [HttpPut("{id:int}", Name = "updateAddress")]
    public async Task<ActionResult<AddressDto>> Update(int id, UpdateAddressDto addressDto, CancellationToken cancellationToken = default)
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Адресу не знайдено", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }

        // Перевіряємо, чи адреса належить поточному користувачу
        if (address.UserId != userId)
        {
            return Forbid();
        }

        // Валідація існування зовнішніх ключів
        var region = await unitOfWork.Regions.GetByIdAsync(addressDto.RegionId);
        if (region == null)
        {
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Вказана область не існує", Status = StatusCodes.Status400BadRequest });
        }

        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(addressDto.AddressTypeId);
        if (addressType == null)
        {
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = "Вказаний тип адреси не існує", Status = StatusCodes.Status400BadRequest });
        }

        // Якщо ця адреса стає основною — зняти прапор у інших
        if (addressDto.IsPrimary)
        {
            var currentUserAddresses = await unitOfWork.Addresses.GetUserAddressesAsync(userId, cancellationToken);
            foreach (var addr in currentUserAddresses.Where(a => a.IsPrimary && a.Id != id))
            {
                addr.IsPrimary = false;
                unitOfWork.Addresses.Update(addr);
            }
        }

        // Оновлення полів (UserId не змінюємо)
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
            UpdatedAt = address.UpdatedAt,
        };

        return Ok(updatedAddressDto);
    }

    [HttpDelete("{id:int}", Name = "deleteAddress")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        // Отримуємо ID поточного користувача з JWT токена
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Адресу не знайдено", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }

        // Перевіряємо, чи адреса належить поточному користувачу
        if (address.UserId != userId)
        {
            return Forbid();
        }

        unitOfWork.Addresses.Delete(address);
        await unitOfWork.CompleteAsync();

        return NoContent();
    }
}
