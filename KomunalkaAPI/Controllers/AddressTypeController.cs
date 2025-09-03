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
    public async Task<ActionResult<ApiResponse<List<AddressTypeDto>>>> GetAll(CancellationToken cancellationToken = default)
    {
        var addressTypes = await unitOfWork.AddressTypes.GetAllAsync();
        var addressTypeList = addressTypes.ToList();

        if (!addressTypeList.Any())
        {
            return NotFound(ApiResponse<List<AddressTypeDto>>.Fail(new[] { "Типи адрес не знайдено" }, "Not Found"));
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

        return Ok(ApiResponse<List<AddressTypeDto>>.Success(addressTypeDtos, "Типи адрес отримано"));
    }

    [HttpGet("{id:int}", Name = "addressType")]
    public async Task<ActionResult<ApiResponse<AddressTypeDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var addressType = await unitOfWork.AddressTypes.GetByIdAsync(id);

        if (addressType == null)
        {
            return NotFound(ApiResponse<AddressTypeDto>.Fail(new[] { "Тип адреси не знайдено" }, "Not Found"));
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

        return Ok(ApiResponse<AddressTypeDto>.Success(addressTypeDto, "Тип адреси отримано"));
    }
}
