using System.Security.Claims;
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        var userAddresses = await _unitOfWork.UserAddresses.GetByUserIdAsync(userId);
        var addressIds = userAddresses.Select(ua => ua.AddressId);

        var providers = await _unitOfWork.ServiceProviders.GetByAddressIdsAsync(addressIds);
        var response = providers.Select(MapToDto);

        return Ok(new ApiResponse<IEnumerable<ServiceProviderWithTariffsDto>> { Data = response });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        var provider = await LoadProviderWithTariffs(id);
        if (provider == null)
            return NotFound(new { error = "Service provider not found" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, provider.AddressId))
            return Forbid();

        return Ok(new ApiResponse<ServiceProviderWithTariffsDto> { Data = MapToDto(provider) });
    }

    [HttpGet("address/{addressId}")]
    public async Task<IActionResult> GetByAddressId(int addressId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, addressId))
            return Forbid();

        var providers = await _unitOfWork.ServiceProviders.GetByAddressIdAsync(addressId);
        var response = providers.Select(MapToDto);

        return Ok(new ApiResponse<IEnumerable<ServiceProviderWithTariffsDto>> { Data = response });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateServiceProviderDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, dto.AddressId))
            return Forbid();

        var serviceProvider = new ServiceProviderModel
        {
            AddressId = dto.AddressId,
            UtilityTypeId = dto.UtilityTypeId,
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

        foreach (var tariffDto in dto.Tariffs)
        {
            var tariff = new TariffModel
            {
                ServiceProviderId = serviceProvider.Id,
                UtilityTypeId = tariffDto.UtilityTypeId ?? dto.UtilityTypeId,
                CurrencyId = tariffDto.CurrencyId ?? 1,
                PricingModel = tariffDto.PricingModel,
                BaseRate = tariffDto.BaseRate,
                ServiceFee = tariffDto.ServiceFee,
                EffectiveFrom = tariffDto.EffectiveFrom ?? DateTime.UtcNow,
                EffectiveTo = tariffDto.EffectiveTo,
                Notes = tariffDto.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.Tariffs.AddAsync(tariff);
        }

        if (dto.Tariffs.Any())
            await _unitOfWork.CompleteAsync();

        var createdProvider = await LoadProviderWithTariffs(serviceProvider.Id);

        return CreatedAtAction(nameof(GetById), new { id = serviceProvider.Id },
            new ApiResponse<ServiceProviderWithTariffsDto> { Data = MapToDto(createdProvider!) });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceProviderDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        var provider = await _unitOfWork.ServiceProviders.GetByIdAsync(id);
        if (provider == null)
            return NotFound(new { error = "Service provider not found" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, provider.AddressId))
            return Forbid();

        if (dto.Name != null) provider.Name = dto.Name;
        if (dto.Description != null) provider.Description = dto.Description;
        if (dto.Phone != null) provider.Phone = dto.Phone;
        if (dto.Email != null) provider.Email = dto.Email;
        if (dto.Website != null) provider.Website = dto.Website;
        if (dto.IsActive.HasValue) provider.IsActive = dto.IsActive.Value;
        if (dto.UtilityTypeId.HasValue) provider.UtilityTypeId = dto.UtilityTypeId.Value;
        provider.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.ServiceProviders.Update(provider);
        await _unitOfWork.CompleteAsync();

        var updatedProvider = await LoadProviderWithTariffs(id);

        return Ok(new ApiResponse<ServiceProviderWithTariffsDto> { Data = MapToDto(updatedProvider!) });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        var provider = await _unitOfWork.ServiceProviders.GetByIdAsync(id);
        if (provider == null)
            return NotFound(new { error = "Service provider not found" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, provider.AddressId))
            return Forbid();

        _unitOfWork.ServiceProviders.Delete(provider);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    [HttpGet("{id}/tariffs")]
    public async Task<IActionResult> GetTariffs(int id)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        var provider = await _unitOfWork.ServiceProviders.GetByIdAsync(id);
        if (provider == null)
            return NotFound(new { error = "Service provider not found" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, provider.AddressId))
            return Forbid();

        var tariffs = await _unitOfWork.Tariffs.GetByServiceProviderIdAsync(id);
        var response = tariffs.Select(MapToTariffDto);

        return Ok(new ApiResponse<IEnumerable<TariffDto>> { Data = response });
    }

    [HttpPost("{id}/tariffs")]
    public async Task<IActionResult> CreateTariff(int id, [FromBody] CreateTariffDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        var provider = await _unitOfWork.ServiceProviders.GetByIdAsync(id);
        if (provider == null)
            return NotFound(new { error = "Service provider not found" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, provider.AddressId))
            return Forbid();

        var tariff = new TariffModel
        {
            ServiceProviderId = id,
            UtilityTypeId = dto.UtilityTypeId ?? provider.UtilityTypeId,
            CurrencyId = dto.CurrencyId ?? 1,
            PricingModel = dto.PricingModel,
            BaseRate = dto.BaseRate,
            ServiceFee = dto.ServiceFee,
            EffectiveFrom = dto.EffectiveFrom ?? DateTime.UtcNow,
            EffectiveTo = dto.EffectiveTo,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tariffs.AddAsync(tariff);
        await _unitOfWork.CompleteAsync();

        var created = await _unitOfWork.Tariffs.GetByIdWithDetailsAsync(tariff.Id);

        return CreatedAtAction(nameof(GetTariffs), new { id },
            new ApiResponse<TariffDto> { Data = MapToTariffDto(created!) });
    }

    [HttpPut("{id}/tariffs/{tariffId}")]
    public async Task<IActionResult> UpdateTariff(int id, int tariffId, [FromBody] UpdateTariffDto dto)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        var provider = await _unitOfWork.ServiceProviders.GetByIdAsync(id);
        if (provider == null)
            return NotFound(new { error = "Service provider not found" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, provider.AddressId))
            return Forbid();

        var tariff = await _unitOfWork.Tariffs.GetByIdWithDetailsAsync(tariffId);
        if (tariff == null || tariff.ServiceProviderId != id)
            return NotFound(new { error = "Tariff not found for this provider" });

        if (dto.UtilityTypeId.HasValue) tariff.UtilityTypeId = dto.UtilityTypeId.Value;
        if (dto.CurrencyId.HasValue) tariff.CurrencyId = dto.CurrencyId.Value;
        if (dto.PricingModel != null) tariff.PricingModel = dto.PricingModel;
        if (dto.BaseRate.HasValue) tariff.BaseRate = dto.BaseRate.Value;
        if (dto.ServiceFee.HasValue) tariff.ServiceFee = dto.ServiceFee.Value;
        if (dto.EffectiveFrom.HasValue) tariff.EffectiveFrom = dto.EffectiveFrom.Value;
        if (dto.EffectiveTo.HasValue) tariff.EffectiveTo = dto.EffectiveTo.Value;
        if (dto.Notes != null) tariff.Notes = dto.Notes;
        tariff.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Tariffs.Update(tariff);
        await _unitOfWork.CompleteAsync();

        var updated = await _unitOfWork.Tariffs.GetByIdWithDetailsAsync(tariffId);

        return Ok(new ApiResponse<TariffDto> { Data = MapToTariffDto(updated!) });
    }

    [HttpDelete("{id}/tariffs/{tariffId}")]
    public async Task<IActionResult> DeleteTariff(int id, int tariffId)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized(new { error = "Invalid user credentials" });

        var provider = await _unitOfWork.ServiceProviders.GetByIdAsync(id);
        if (provider == null)
            return NotFound(new { error = "Service provider not found" });

        if (!await _unitOfWork.UserAddresses.UserHasAccessToAddressAsync(userId, provider.AddressId))
            return Forbid();

        var tariff = await _unitOfWork.Tariffs.GetByIdAsync(tariffId);
        if (tariff == null || tariff.ServiceProviderId != id)
            return NotFound(new { error = "Tariff not found for this provider" });

        _unitOfWork.Tariffs.Delete(tariff);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }

    private bool TryGetUserId(out int userId)
    {
        userId = 0;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return !string.IsNullOrEmpty(claim) && int.TryParse(claim, out userId);
    }

    private async Task<ServiceProviderModel?> LoadProviderWithTariffs(int id)
    {
        return await _unitOfWork.GetContext()
            .Set<ServiceProviderModel>()
            .Include(sp => sp.UtilityType)
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.UtilityType)
            .Include(sp => sp.Tariffs)
                .ThenInclude(t => t.Currency)
            .FirstOrDefaultAsync(sp => sp.Id == id);
    }

    private static TariffDto MapToTariffDto(TariffModel t)
    {
        return new TariffDto
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
        };
    }

    private static ServiceProviderWithTariffsDto MapToDto(ServiceProviderModel sp)
    {
        return new ServiceProviderWithTariffsDto
        {
            Id = sp.Id,
            AddressId = sp.AddressId,
            UtilityTypeId = sp.UtilityTypeId,
            UtilityTypeName = sp.UtilityType?.DisplayName,
            Name = sp.Name,
            Description = sp.Description,
            Phone = sp.Phone,
            Email = sp.Email,
            Website = sp.Website,
            IsActive = sp.IsActive,
            CreatedAt = sp.CreatedAt,
            UpdatedAt = sp.UpdatedAt,
            Tariffs = sp.Tariffs.Select(MapToTariffDto).ToList()
        };
    }
}
