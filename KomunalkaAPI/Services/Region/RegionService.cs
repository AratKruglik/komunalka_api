using KomunalkaAPI.DTO;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.Region;

public class RegionService(IUnitOfWork unitOfWork) : IRegionService
{
    public async Task<IReadOnlyList<RegionDto>> GetAllAsync()
    {
        var regions = await unitOfWork.Regions.GetAllAsync();
        var list = regions.ToList();
        var dtos = list.Select(region => new RegionDto
        {
            Id = region.Id,
            Name = region.Name,
            CreatedAt = region.CreatedAt,
            UpdatedAt = region.UpdatedAt
        }).ToList();
        return dtos;
    }

    public async Task<ServiceResult<RegionDto>> GetByIdAsync(int id)
    {
        var region = await unitOfWork.Regions.GetByIdAsync(id);
        if (region == null)
        {
            return ServiceResult<RegionDto>.NotFoundResult("Область не знайдено");
        }

        var dto = new RegionDto
        {
            Id = region.Id,
            Name = region.Name,
            CreatedAt = region.CreatedAt,
            UpdatedAt = region.UpdatedAt
        };
        return ServiceResult<RegionDto>.Ok(dto);
    }
}