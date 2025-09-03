using KomunalkaAPI.DTO;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressTypeController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet(Name = "addressTypes")]
    public async Task<ActionResult<IEnumerable<AddressTypeDto>>> GetAll(CancellationToken cancellationToken = default)
    {
        var addressTypes = await unitOfWork.AddressTypes.GetAllAsync();
        var addressTypeList = addressTypes.ToList();

        if (!addressTypeList.Any())
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Типи адрес не знайдено", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }

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
    public async Task<ActionResult<AddressTypeDto>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(id);

        if (addressType == null)
        {
            return NotFound(new ProblemDetails { Title = "Not Found", Detail = "Тип адреси не знайдено", Status = StatusCodes.Status404NotFound, Instance = HttpContext.Request.Path });
        }

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
