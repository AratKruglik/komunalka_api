using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;

namespace KomunalkaAPI.Services.Tariff;

public interface ITariffCalculationService
{
    Task<Models.Tariff?> GetEffectiveTariffAsync(int meterId, DateTime readingDate);
    Task<TariffCalculationDto?> CalculateCostAsync(Meter meter, decimal consumption, DateTime readingDate, int? tariffId = null);
    Task<List<TariffCalculationDto>> CalculateBatchCostsAsync(List<(Meter meter, decimal consumption, DateTime readingDate, int? tariffId)> readings);
}
