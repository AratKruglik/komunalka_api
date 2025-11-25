using KomunalkaAPI.DTO;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceProviderModel = KomunalkaAPI.Models.ServiceProvider;
using TariffModel = KomunalkaAPI.Models.Tariff;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/service-providers")]
[Asp.Versioning.ApiVersion("1.0")]
[Authorize]
public class ServiceProvidersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ServiceProvidersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Create a new service provider with tariffs
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceProviderDto dto)
    {
        // Create service provider
        var serviceProvider = new ServiceProviderModel
        {
            Name = dto.Name,
            Description = dto.Description,
            Phone = dto.Phone,
            Email = dto.Email,
            Website = dto.Website,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.ServiceProviders.AddAsync(serviceProvider);
        await _unitOfWork.CompleteAsync();

        // Create tariffs for the service provider
        var tariffs = new List<TariffModel>();
        foreach (var tariffDto in dto.Tariffs)
        {
            var tariff = new TariffModel
            {
                ServiceProviderId = serviceProvider.Id,
                UtilityTypeId = tariffDto.UtilityTypeId,
                CurrencyId = tariffDto.CurrencyId,
                PricingModel = tariffDto.PricingModel,
                BaseRate = tariffDto.BaseRate,
                ServiceFee = tariffDto.ServiceFee,
                EffectiveFrom = tariffDto.EffectiveFrom,
                EffectiveTo = tariffDto.EffectiveTo,
                Notes = tariffDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            tariffs.Add(tariff);
        }

        if (tariffs.Any())
        {
            var context = _unitOfWork.GetContext();
            await context.Set<TariffModel>().AddRangeAsync(tariffs);
            await _unitOfWork.CompleteAsync();
        }

        // Load created service provider with tariffs
        var createdProvider = await _unitOfWork.GetContext()
            .Set<ServiceProviderModel>()
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.UtilityType)
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.Currency)
            .FirstOrDefaultAsync(sp => sp.Id == serviceProvider.Id);

        if (createdProvider == null)
        {
            return StatusCode(500, new { message = "Failed to retrieve created service provider" });
        }

        // Map to DTO
        var response = new ServiceProviderWithTariffsDto
        {
            Id = createdProvider.Id,
            Name = createdProvider.Name,
            Description = createdProvider.Description,
            Phone = createdProvider.Phone,
            Email = createdProvider.Email,
            Website = createdProvider.Website,
            IsActive = createdProvider.IsActive,
            CreatedAt = createdProvider.CreatedAt,
            UpdatedAt = createdProvider.UpdatedAt,
            Tariffs = createdProvider.Tariffs.Select(t => new TariffDto
            {
                Id = t.Id,
                ServiceProviderId = t.ServiceProviderId,
                UtilityTypeId = t.UtilityTypeId,
                CurrencyId = t.CurrencyId,
                PricingModel = t.PricingModel,
                BaseRate = t.BaseRate,
                ServiceFee = t.ServiceFee,
                EffectiveFrom = t.EffectiveFrom,
                EffectiveTo = t.EffectiveTo,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                UtilityTypeName = t.UtilityType?.DisplayName,
                CurrencyCode = t.Currency?.Code,
                CurrencySymbol = t.Currency?.Symbol
            }).ToList()
        };

        return CreatedAtAction(nameof(GetById), new { id = serviceProvider.Id }, new { data = response });
    }

    /// <summary>
    /// Get service provider by ID with tariffs
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var serviceProvider = await _unitOfWork.GetContext()
            .Set<ServiceProviderModel>()
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.UtilityType)
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.Currency)
            .FirstOrDefaultAsync(sp => sp.Id == id);

        if (serviceProvider == null)
        {
            return NotFound(new { message = "Service provider not found" });
        }

        var response = new ServiceProviderWithTariffsDto
        {
            Id = serviceProvider.Id,
            Name = serviceProvider.Name,
            Description = serviceProvider.Description,
            Phone = serviceProvider.Phone,
            Email = serviceProvider.Email,
            Website = serviceProvider.Website,
            IsActive = serviceProvider.IsActive,
            CreatedAt = serviceProvider.CreatedAt,
            UpdatedAt = serviceProvider.UpdatedAt,
            Tariffs = serviceProvider.Tariffs.Select(t => new TariffDto
            {
                Id = t.Id,
                ServiceProviderId = t.ServiceProviderId,
                UtilityTypeId = t.UtilityTypeId,
                CurrencyId = t.CurrencyId,
                PricingModel = t.PricingModel,
                BaseRate = t.BaseRate,
                ServiceFee = t.ServiceFee,
                EffectiveFrom = t.EffectiveFrom,
                EffectiveTo = t.EffectiveTo,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                UtilityTypeName = t.UtilityType?.DisplayName,
                CurrencyCode = t.Currency?.Code,
                CurrencySymbol = t.Currency?.Symbol
            }).ToList()
        };

        return Ok(new { data = response });
    }

    /// <summary>
    /// Get all service providers
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var serviceProviders = await _unitOfWork.GetContext()
            .Set<ServiceProviderModel>()
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.UtilityType)
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.Currency)
            .ToListAsync();

        var response = serviceProviders.Select(sp => new ServiceProviderWithTariffsDto
        {
            Id = sp.Id,
            Name = sp.Name,
            Description = sp.Description,
            Phone = sp.Phone,
            Email = sp.Email,
            Website = sp.Website,
            IsActive = sp.IsActive,
            CreatedAt = sp.CreatedAt,
            UpdatedAt = sp.UpdatedAt,
            Tariffs = sp.Tariffs.Select(t => new TariffDto
            {
                Id = t.Id,
                ServiceProviderId = t.ServiceProviderId,
                UtilityTypeId = t.UtilityTypeId,
                CurrencyId = t.CurrencyId,
                PricingModel = t.PricingModel,
                BaseRate = t.BaseRate,
                ServiceFee = t.ServiceFee,
                EffectiveFrom = t.EffectiveFrom,
                EffectiveTo = t.EffectiveTo,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                UtilityTypeName = t.UtilityType?.DisplayName,
                CurrencyCode = t.Currency?.Code,
                CurrencySymbol = t.Currency?.Symbol
            }).ToList()
        }).ToList();

        return Ok(new { data = response });
    }
}
