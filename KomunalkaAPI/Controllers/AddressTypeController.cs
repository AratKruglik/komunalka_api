using KomunalkaAPI.DTO;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize]
public class AddressTypeController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet(Name = "addressTypes")]
    public async Task<ActionResult<IEnumerable<AddressTypeDto>>> GetAll()
    {
        var addressTypes = await unitOfWork.AddressTypes.GetAllAsync();
        var addressTypeList = addressTypes.ToList();

        if (!addressTypeList.Any())
        {
            return NotFound("Типи адрес не знайдено");
        }

        await unitOfWork.CompleteAsync();

        var addressTypeDtos = addressTypeList.Select(addressType => new AddressTypeDto
        {
            Id = addressType.Id,
            Name = addressType.Name,
            Description = addressType.Description,
            Icon = addressType.Icon,
            CreatedAt = addressType.CreatedAt,
            UpdatedAt = addressType.UpdatedAt
        }).ToList();

        return Ok(addressTypeDtos);
    }

    [HttpGet("{id:int}", Name = "addressType")]
    public async Task<ActionResult<AddressTypeDto>> GetById(int id)
    {
        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(id);

        if (addressType == null)
        {
            return NotFound("Тип адреси не знайдено");
        }

        await unitOfWork.CompleteAsync();

        var addressTypeDto = new AddressTypeDto
        {
            Id = addressType.Id,
            Name = addressType.Name,
            Description = addressType.Description,
            Icon = addressType.Icon,
            CreatedAt = addressType.CreatedAt,
            UpdatedAt = addressType.UpdatedAt
        };

        return Ok(addressTypeDto);
    }
}
