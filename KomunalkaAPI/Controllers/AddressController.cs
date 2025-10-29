using KomunalkaAPI.DTO;
using KomunalkaAPI.Extensions;
using KomunalkaAPI.Models.Pagination;
using KomunalkaAPI.Models.Responses;
using KomunalkaAPI.Services.Address;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize]
public class AddressController(IAddressService addressService) : ControllerBase
{
    [HttpGet(Name = "addresses")]
    public async Task<ActionResult<PaginatedResponse<AddressDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int perPage = 15,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool desc = false,
        CancellationToken cancellationToken = default)
    {
        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        var skip = (page - 1) * perPage;
        var (items, totalCount, lastUpdatedFileTimeUtc) = await addressService.GetForUserAsync(
            userId, skip, perPage, sortBy, desc, cancellationToken);

        var pagedResult = new PagedResult<AddressDto>
        {
            Items = items.ToList(),
            PageNumber = page,
            PageSize = perPage,
            TotalCount = totalCount
        };

        // Convert to Laravel-compatible format
        var response = pagedResult.ToPaginatedResponse(items.ToList(), Request);

        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "address")]
    public async Task<ActionResult<AddressDto>> GetById(int id, CancellationToken cancellationToken)
    {
        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        var result = await addressService.GetByIdForUserAsync(userId, id, cancellationToken);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors.FirstOrDefault());
            if (result.Forbidden)
                return Forbid();
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpPost(Name = "createAddress")]
    public async Task<ActionResult<AddressDto>> Create(CreateAddressDto createAddressDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        var result = await addressService.CreateAsync(userId, createAddressDto, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(result.Errors);
        }

        return CreatedAtRoute("address", new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}", Name = "updateAddress")]
    public async Task<ActionResult<AddressDto>> Update(int id, UpdateAddressDto updateAddressDto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        var result = await addressService.UpdateAsync(userId, id, updateAddressDto, cancellationToken);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors.FirstOrDefault());
            if (result.Forbidden)
                return Forbid();
            return BadRequest(result.Errors);
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}", Name = "deleteAddress")]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        var result = await addressService.DeleteAsync(userId, id, cancellationToken);

        if (!result.Success)
        {
            if (result.NotFound)
                return NotFound(result.Errors.FirstOrDefault());
            if (result.Forbidden)
                return Forbid();
            return BadRequest(result.Errors);
        }

        return NoContent();
    }
}
