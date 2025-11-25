namespace KomunalkaAPI.Repositories.MeterReading;

public interface IMeterReadingRepository : IRepository<Models.MeterReading>
{
    Task<IEnumerable<Models.MeterReading>> GetByMeterIdAsync(int meterId);
    Task<IEnumerable<Models.MeterReading>> GetByAddressIdAsync(int addressId);
    Task<Models.MeterReading?> GetLatestByMeterIdAsync(int meterId);
    Task<IEnumerable<Models.MeterReading>> GetByDateRangeAsync(int meterId, DateTime from, DateTime to);
    Task<IEnumerable<Models.MeterReading>> GetByAddressAndDateRangeAsync(int addressId, DateTime from, DateTime to);
    Task<bool> HasReadingOnDateAsync(int meterId, DateTime date);
}
