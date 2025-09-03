using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Region;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RegionController(IRegionService regionService) : ControllerBase
{
    [HttpGet(Name = "regions")]
    public async Task<ActionResult<ApiResponse<List<RegionDto>>>> GetAll()
    {
        var regions = await regionService.GetAllAsync();
        if (!regions.Any())
        {
            return NotFound(ApiResponse<List<RegionDto>>.Fail(new[] { "Regions not found" }, "Not Found"));
        }

        return Ok(ApiResponse<List<RegionDto>>.Success(regions.ToList(), "Regions retrieved"));
    }

    [HttpGet("{id:int}", Name = "region")]
    public async Task<ActionResult<ApiResponse<RegionDto>>> GetById(int id)
    {
        var result = await regionService.GetByIdAsync(id);

        if (result.NotFound)
        {
            return NotFound(ApiResponse<RegionDto>.Fail(new[] { "Region not found" }, "Not Found"));
        }

        return Ok(ApiResponse<RegionDto>.Success(result.Data!, "Region retrieved"));
    }
}
