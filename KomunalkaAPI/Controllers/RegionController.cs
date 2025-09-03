using KomunalkaAPI.DTO;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RegionController(IUnitOfWork unitOfWork) : ControllerBase
{
    [HttpGet(Name = "regions")]
    public async Task<ActionResult<IEnumerable<RegionDto>>> GetAll()
    {
        var regions = await unitOfWork.Regions.GetAllAsync();
        var regionList = regions.ToList();

        if (!regionList.Any())
        {
            return NotFound("Області не знайдено");
        }

        await unitOfWork.CompleteAsync();

        var regionDtos = regionList.Select(region => new RegionDto
        {
            Id = region.Id,
            Name = region.Name,
            CreatedAt = region.CreatedAt,
            UpdatedAt = region.UpdatedAt
        }).ToList();

        return Ok(regionDtos);
    }

    [HttpGet("{id:int}", Name = "region")]
    public async Task<ActionResult<RegionDto>> GetById(int id)
    {
        var region = await unitOfWork.Regions.GetByIdAsync(id);

        if (region == null)
        {
            return NotFound("Область не знайдено");
        }

        await unitOfWork.CompleteAsync();

        var regionDto = new RegionDto
        {
            Id = region.Id,
            Name = region.Name,
            CreatedAt = region.CreatedAt,
            UpdatedAt = region.UpdatedAt
        };

        return Ok(regionDto);
    }
}
