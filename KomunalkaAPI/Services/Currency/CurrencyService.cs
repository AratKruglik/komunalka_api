using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using KomunalkaAPI.Services.Common;

namespace KomunalkaAPI.Services.Currency;

public class CurrencyService(IUnitOfWork unitOfWork) : ICurrencyService
{
    public async Task<IReadOnlyList<CurrencyDto>> GetAllAsync()
    {
        var currencies = await unitOfWork.Currencies.GetAllAsync();
        var list = currencies.ToList();
        var dtos = list.Select(c => new CurrencyDto
        {
            Id = c.Id,
            Code = c.Code,
            Name = c.Name,
            Symbol = c.Symbol,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
        return dtos;
    }

    public async Task<ServiceResult<CurrencyDto>> GetByIdAsync(int id)
    {
        var currency = await unitOfWork.Currencies.GetByIdAsync(id);
        if (currency == null)
        {
            return ServiceResult<CurrencyDto>.NotFoundResult("Валюту не знайдено");
        }

        var dto = new CurrencyDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            CreatedAt = currency.CreatedAt,
            UpdatedAt = currency.UpdatedAt
        };
        return ServiceResult<CurrencyDto>.Ok(dto);
    }

    public async Task<CurrencyDto> CreateAsync(CreateCurrencyDto dto)
    {
        var entity = new Models.Currency
        {
            Code = dto.Code,
            Name = dto.Name,
            Symbol = dto.Symbol,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var entry = await unitOfWork.Currencies.AddAsync(entity);
        await unitOfWork.CompleteAsync();

        var created = entry.Entity;
        return new CurrencyDto
        {
            Id = created.Id,
            Code = created.Code,
            Name = created.Name,
            Symbol = created.Symbol,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        };
    }

    public async Task<ServiceResult<CurrencyDto>> UpdateAsync(int id, UpdateCurrencyDto dto)
    {
        var currency = await unitOfWork.Currencies.GetByIdAsync(id);
        if (currency == null)
        {
            return ServiceResult<CurrencyDto>.NotFoundResult("Валюту не знайдено");
        }

        currency.Code = dto.Code;
        currency.Name = dto.Name;
        currency.Symbol = dto.Symbol;
        currency.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Currencies.Update(currency);
        await unitOfWork.CompleteAsync();

        var updatedDto = new CurrencyDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            CreatedAt = currency.CreatedAt,
            UpdatedAt = currency.UpdatedAt
        };
        return ServiceResult<CurrencyDto>.Ok(updatedDto);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var currency = await unitOfWork.Currencies.GetByIdAsync(id);
        if (currency == null)
        {
            return ServiceResult<bool>.NotFoundResult("Валюту не знайдено");
        }

        unitOfWork.Currencies.Delete(currency);
        await unitOfWork.CompleteAsync();
        return ServiceResult<bool>.Ok(true);
    }
}