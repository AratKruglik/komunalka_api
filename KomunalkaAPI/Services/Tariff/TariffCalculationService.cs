using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KomunalkaAPI.Services.Tariff;

public class TariffCalculationService : ITariffCalculationService
{
    private readonly IUnitOfWork _unitOfWork;

    public TariffCalculationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Models.Tariff?> GetEffectiveTariffAsync(int meterId, DateTime readingDate)
    {
        var meter = await _unitOfWork.Meters.GetContext()
            .Set<Models.Meter>()
            .Include(m => m.ServiceProvider)
            .Include(m => m.UtilityType)
            .FirstOrDefaultAsync(m => m.Id == meterId);

        if (meter == null || meter.ServiceProviderId == null) return null;

        var tariffs = await _unitOfWork.Meters.GetContext()
            .Set<Models.Tariff>()
            .Include(t => t.Currency)
            .Where(t => t.ServiceProviderId == meter.ServiceProviderId &&
                       t.UtilityTypeId == meter.UtilityTypeId)
            .ToListAsync();

        var effectiveTariff = tariffs
            .Where(t => t.EffectiveFrom <= readingDate &&
                       (t.EffectiveTo == null || t.EffectiveTo >= readingDate))
            .OrderByDescending(t => t.EffectiveFrom)
            .FirstOrDefault();

        return effectiveTariff;
    }

    public async Task<TariffCalculationDto?> CalculateCostAsync(Meter meter, decimal consumption, DateTime readingDate, int? tariffId = null)
    {
        Models.Tariff? tariff;

        if (tariffId.HasValue)
        {
            tariff = await _unitOfWork.Tariffs.GetByIdWithDetailsAsync(tariffId.Value);
        }
        else
        {
            tariff = await GetEffectiveTariffAsync(meter.Id, readingDate);
        }

        if (tariff == null) return null;

        var consumptionCost = consumption * tariff.BaseRate;
        var serviceFeeCost = tariff.ServiceFee ?? 0;
        var totalCost = consumptionCost + serviceFeeCost;

        return new TariffCalculationDto
        {
            MeterId = meter.Id,
            MeterName = meter.Name,
            UtilityType = meter.UtilityType?.DisplayName ?? "Unknown",
            Consumption = consumption,
            Unit = meter.UtilityType?.Unit ?? "unit",
            BaseRate = tariff.BaseRate,
            ServiceFee = tariff.ServiceFee,
            CurrencyCode = tariff.Currency?.Code ?? "UAH",
            CurrencySymbol = tariff.Currency?.Symbol ?? "₴",
            TotalCost = totalCost,
            TariffIdentifier = $"{tariff.PricingModel}-{tariff.Id}",
            TariffEffectiveFrom = tariff.EffectiveFrom,
            TariffEffectiveTo = tariff.EffectiveTo,
            ConsumptionCost = consumptionCost,
            ServiceFeeCost = serviceFeeCost
        };
    }

    public async Task<List<TariffCalculationDto>> CalculateBatchCostsAsync(
        List<(Meter meter, decimal consumption, DateTime readingDate, int? tariffId)> readings)
    {
        var calculations = new List<TariffCalculationDto>();

        foreach (var (meter, consumption, readingDate, tariffId) in readings)
        {
            var calculation = await CalculateCostAsync(meter, consumption, readingDate, tariffId);
            if (calculation != null)
            {
                calculations.Add(calculation);
            }
        }

        return calculations;
    }
}
