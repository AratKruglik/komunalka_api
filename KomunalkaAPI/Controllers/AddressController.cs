using AutoMapper;
using KomunalkaAPI.Extensions;
using KomunalkaAPI.Models.Pagination;
using KomunalkaAPI.Models.Responses;
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
    public async Task<ActionResult<PaginatedResponse<AddressDto>>> GetAll([FromQuery] PaginationParams paginationParams)
    {
        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        // Get all addresses with pagination
        var pagedAddresses = await unitOfWork.Addresses.GetPagedAsync(paginationParams);

        // Filter addresses only for current user
        var userAddresses = pagedAddresses.Items.Where(a => a.UserId == userId).ToList();
        var addressDtos = mapper.Map<List<AddressDto>>(userAddresses);

        var pagedResult = new PagedResult<AddressDto>
        {
            Items = addressDtos,
            PageNumber = pagedAddresses.PageNumber,
            PageSize = pagedAddresses.PageSize,
            TotalCount = pagedAddresses.Items.Count(a => a.UserId == userId) // Count of user addresses
        };

        // Convert to Laravel-compatible format
        var response = pagedResult.ToPaginatedResponse(addressDtos, Request);

        return Ok(response);
    }

    [HttpGet("{id:int}", Name = "address")]
    public async Task<ActionResult<AddressDto>> GetById(int id)
    {
        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound("Address not found");
        }

        // Check if the address belongs to the current user
        if (address.UserId != userId)
        {
            return Forbid("You don't have access to this address");
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

        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        // Check if Region and AddressType exist
        var region = await unitOfWork.Regions.GetByIdAsync(createAddressDto.RegionId);
        if (region == null)
        {
            return BadRequest("Specified region does not exist");
        }

        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(createAddressDto.AddressTypeId);
        if (addressType == null)
        {
            return BadRequest("Specified address type does not exist");
        }

        // If this is primary address, set all other user addresses as non-primary
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
        address.User = null!; // Will be populated by EF
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
        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound("Address not found");
        }

        // Check if the address belongs to the current user
        if (address.UserId != userId)
        {
            return Forbid("You don't have access to this address");
        }

        // Don't allow changing UserId - address always belongs to current user
        mapper.Map(addressDto, address);
        address.UserId = userId; // Ensure UserId doesn't change
        address.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Addresses.Update(address);
        await unitOfWork.CompleteAsync();

        var updatedAddressDto = mapper.Map<AddressDto>(address);

        return Ok(updatedAddressDto);
    }

    [HttpDelete("{id:int}", Name = "deleteAddress")]
    public async Task<ActionResult> Delete(int id)
    {
        // Get current user ID from JWT token
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid user token");
        }

        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound("Address not found");
        }

        // Check if the address belongs to the current user
        if (address.UserId != userId)
        {
            return Forbid("You don't have access to this address");
        }

        unitOfWork.Addresses.Delete(address);
        await unitOfWork.CompleteAsync();

        return NoContent();
    }
}
