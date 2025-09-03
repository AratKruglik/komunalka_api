using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.AddressType;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressTypeController(IAddressTypeService addressTypeService) : ControllerBase
{
    [HttpGet(Name = "addressTypes")]
    public async Task<ActionResult<ApiResponse<List<AddressTypeDto>>>> GetAll(CancellationToken cancellationToken = default)
    {
        var addressTypeList = await addressTypeService.GetAllAsync();

        if (!addressTypeList.Any())
        {
            return NotFound(ApiResponse<List<AddressTypeDto>>.Fail(new[] { "Address types not found" }, "Not Found"));
        }

        return Ok(ApiResponse<List<AddressTypeDto>>.Success(addressTypeList.ToList(), "Address types retrieved"));
    }

    [HttpGet("{id:int}", Name = "addressType")]
    public async Task<ActionResult<ApiResponse<AddressTypeDto>>> GetById(int id, CancellationToken cancellationToken = default)
    {
        var result = await addressTypeService.GetByIdAsync(id);

        if (result.NotFound)
        {
            return NotFound(ApiResponse<AddressTypeDto>.Fail(new[] { "Address type not found" }, "Not Found"));
        }

        return Ok(ApiResponse<AddressTypeDto>.Success(result.Data!, "Address type retrieved"));
    }
}
