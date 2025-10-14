using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.Region;

public interface IRegionService
{
    Task<IReadOnlyList<RegionDto>> GetAllAsync();
    Task<ServiceResult<RegionDto>> GetByIdAsync(int id);
}