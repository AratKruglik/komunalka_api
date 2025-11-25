using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;

namespace KomunalkaAPI.Services.Tariff;

public interface ITariffCalculationService
{
    Task<Models.Tariff?> GetEffectiveTariffAsync(int meterId, DateTime readingDate);
    Task<TariffCalculationDto?> CalculateCostAsync(Meter meter, decimal consumption, DateTime readingDate);
    Task<List<TariffCalculationDto>> CalculateBatchCostsAsync(List<(Meter meter, decimal consumption, DateTime readingDate)> readings);
}
