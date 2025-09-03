using Microsoft.AspNetCore.Mvc;
using KomunalkaAPI.DTO;
using KomunalkaAPI.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using KomunalkaAPI.Services.Address;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressController(IAddressService addressService) : ControllerBase
{
    [HttpGet(Name = "addresses")]
    public async Task<ActionResult<ApiResponse<List<AddressDto>>>> GetAll(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return StatusCode(StatusCodes.Status401Unauthorized, ApiResponse<List<AddressDto>>.Fail(new[] { "Невійсний токен користувача" }, "Unauthorized"));
        }

        var (items, totalCount, lastUpdatedTicks) = await addressService.GetForUserAsync(userId, skip, take, sortBy, desc, cancellationToken);
        if (totalCount == 0)
        {
            return NotFound(ApiResponse<List<AddressDto>>.Fail(new[] { "У вас ще немає збережених адрес" }, "Not Found"));
        }

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

        var meta = new PaginationMeta { Skip = skip, Take = take, Returned = items.Count, Total = totalCount };
        return Ok(ApiResponse<List<AddressDto>>.Success(items.ToList(), "Адреси отримано", meta));
    }

    [HttpGet("{id:int}", Name = "address")]
    public async Task<ActionResult<AddressDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await addressService.GetByIdForUserAsync(userId, id, cancellationToken);
        if (result.NotFound)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Адресу не знайдено", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }
        if (result.Forbidden)
        {
            return Forbid();
        }

        var dto = result.Data!;

        var etag = $"W/\"addr-{dto.Id}-{dto.UpdatedAt.ToFileTimeUtc()}\"";
        var ifNoneMatch = Request.Headers["If-None-Match"].ToString();
        if (!string.IsNullOrEmpty(ifNoneMatch) && string.Equals(ifNoneMatch, etag, StringComparison.Ordinal))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }
        Response.Headers["ETag"] = etag;
        Response.Headers["Cache-Control"] = "private, max-age=60";

        return Ok(dto);
    }

    [HttpPost(Name = "createAddress")]
    public async Task<ActionResult<AddressDto>> Create(CreateAddressDto createAddressDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await addressService.CreateAsync(userId, createAddressDto, cancellationToken);
        if (!result.Success)
        {
            // Business validation failures
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = string.Join("; ", result.Errors), Status = StatusCodes.Status400BadRequest });
        }

        var createdAddressDto = result.Data!;
        return CreatedAtRoute("address", new { id = createdAddressDto.Id }, createdAddressDto);
    }

    [HttpPut("{id:int}", Name = "updateAddress")]
    public async Task<ActionResult<AddressDto>> Update(int id, UpdateAddressDto addressDto, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await addressService.UpdateAsync(userId, id, addressDto, cancellationToken);
        if (result.NotFound)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Адресу не знайдено", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }
        if (result.Forbidden)
        {
            return Forbid();
        }
        if (!result.Success)
        {
            return BadRequest(new ProblemDetails { Title = "Bad Request", Detail = string.Join("; ", result.Errors), Status = StatusCodes.Status400BadRequest });
        }

        return Ok(result.Data!);
    }

    [HttpDelete("{id:int}", Name = "deleteAddress")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Problem(title: "Unauthorized", detail: "Невійсний токен користувача", statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await addressService.DeleteAsync(userId, id, cancellationToken);
        if (result.NotFound)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Адресу не знайдено", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }
        if (result.Forbidden)
        {
            return Forbid();
        }

        return NoContent();
    }
}
