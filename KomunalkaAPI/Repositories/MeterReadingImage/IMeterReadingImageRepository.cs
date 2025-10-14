using KomunalkaAPI.Models;

namespace KomunalkaAPI.Repositories.MeterReadingImage;

public interface IMeterReadingImageRepository : IRepository<Models.MeterReadingImage>
{
    Task<IEnumerable<Models.MeterReadingImage>> GetByServiceCounterValueIdAsync(int serviceCounterValueId);
    Task<Models.MeterReadingImage?> GetWithServiceCounterValueAsync(int id);
}
