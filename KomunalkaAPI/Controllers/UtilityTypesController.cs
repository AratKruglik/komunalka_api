using KomunalkaAPI.DTO;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/utility-types")]
[Asp.Versioning.ApiVersion("1.0")]
public class UtilityTypesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UtilityTypesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var utilityTypes = await _unitOfWork.UtilityTypes.GetActiveAsync();
        var response = utilityTypes.Select(ut => new UtilityTypeDto
        {
            Id = ut.Id,
            Slug = ut.Slug,
            DisplayName = ut.DisplayName,
            Unit = ut.Unit,
            Description = ut.Description,
            IsActive = ut.IsActive
        });

        return Ok(new ApiResponse<IEnumerable<UtilityTypeDto>> { Data = response });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var utilityType = await _unitOfWork.UtilityTypes.GetByIdAsync(id);
        if (utilityType == null)
            return NotFound(new { error = "Utility type not found" });

        var dto = new UtilityTypeDto
        {
            Id = utilityType.Id,
            Slug = utilityType.Slug,
            DisplayName = utilityType.DisplayName,
            Unit = utilityType.Unit,
            Description = utilityType.Description,
            IsActive = utilityType.IsActive
        };

        return Ok(new ApiResponse<UtilityTypeDto> { Data = dto });
    }
}
