using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using KomunalkaAPI.Models;
using KomunalkaAPI.DTO;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AddressController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet(Name = "addresses")]
    public async Task<ActionResult<IEnumerable<AddressDto>>> GetAll()
    {
        var addresses = await unitOfWork.Addresses.GetAllAsync();
        IEnumerable<Address> addressList = addresses.ToList();

        if (!addressList.Any())
        {
            return NotFound();
        }
        
        await unitOfWork.CompleteAsync();

        var addressDtos = addressList.Select(address => new AddressDto
        {
            Id = address.Id,
            UserId = address.UserId,
            ZipCode = address.ZipCode,
            City = address.City,
            Street = address.Street,
            Building = address.Building,
            User = address.User,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        }).ToList();

        return Ok(addressDtos);
    }

    [HttpGet("{id:int}", Name = "address")]
    public async Task<ActionResult<AddressDto>> GetById(int id)
    {
        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound();
        }

        await unitOfWork.CompleteAsync();

        var addressDto = new AddressDto
        {
            Id = address.Id,
            UserId = address.UserId,
            ZipCode = address.ZipCode,
            City = address.City,
            Street = address.Street,
            Building = address.Building,
            User = address.User,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };

        return Ok(addressDto);
    }

    // [HttpPost(Name = "createAddress")]
    // public async Task<ActionResult<AddressDto>> Create(AddressDto addressDto)
    // {
    //     var address = new Address
    //     {
    //         UserId = addressDto.UserId,
    //         ZipCode = addressDto.ZipCode,
    //         City = addressDto.City,
    //         Street = addressDto.Street,
    //         Building = addressDto.Building
    //     };
    //
    //     var entityEntry = await unitOfWork.Addresses.AddAsync(address);
    //     await unitOfWork.CompleteAsync();
    //
    //     var createdAddress = entityEntry.Entity;
    //     var createdAddressDto = new AddressDto
    //     {
    //         Id = createdAddress.Id,
    //         UserId = createdAddress.UserId,
    //         ZipCode = createdAddress.ZipCode,
    //         City = createdAddress.City,
    //         Street = createdAddress.Street,
    //         Building = createdAddress.Building,
    //         CreatedAt = createdAddress.CreatedAt,
    //         UpdatedAt = createdAddress.UpdatedAt,
    //         DeletedAt = createdAddress.DeletedAt
    //     };
    //
    //     return CreatedAtRoute("address", new { id = createdAddressDto.Id }, createdAddressDto);
    // }

    [HttpPut("{id:int}", Name = "updateAddress")]
    public async Task<ActionResult<AddressDto>> Update(int id, AddressDto addressDto)
    {
        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound();
        }

        address.UserId = addressDto.UserId;
        address.ZipCode = addressDto.ZipCode;
        address.City = addressDto.City;
        address.Street = addressDto.Street;
        address.Building = addressDto.Building;
        address.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Addresses.Update(address);
        await unitOfWork.CompleteAsync();

        var updatedAddressDto = new AddressDto
        {
            Id = address.Id,
            UserId = address.UserId,
            ZipCode = address.ZipCode,
            City = address.City,
            Street = address.Street,
            Building = address.Building,
            CreatedAt = address.CreatedAt,
            UpdatedAt = address.UpdatedAt,
        };

        return Ok(updatedAddressDto);
    }

    [HttpDelete("{id:int}", Name = "deleteAddress")]
    public async Task<ActionResult> Delete(int id)
    {
        var address = await unitOfWork.Addresses.GetByIdAsync(id);

        if (address == null)
        {
            return NotFound();
        }

        unitOfWork.Addresses.Delete(address);
        await unitOfWork.CompleteAsync();

        return NoContent();
    }
}
