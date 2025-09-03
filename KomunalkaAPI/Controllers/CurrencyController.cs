using KomunalkaAPI.DTO;
using KomunalkaAPI.Models;
using KomunalkaAPI.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController(IUnitOfWork unitOfWork) : ControllerBase
{
    // GET: api/Currency
    [HttpGet(Name = "currencies")]
    public async Task<ActionResult<ApiResponse<List<CurrencyDto>>>> GetCurrencies()
    {
        var currencies = await unitOfWork.Currencies.GetAllAsync();
        IEnumerable<Currency> currencyList = currencies.ToList();
        
        if (!currencyList.Any())
        {
            return NotFound(ApiResponse<List<CurrencyDto>>.Fail(new[] { "Валюти не знайдено" }, "Not Found"));
        }
        await unitOfWork.CompleteAsync();

        var currencyDtos = currencyList.Select(currency => new CurrencyDto
        {
            Id = currency.Id,
            Name = currency.Name,
            Symbol = currency.Symbol,
            CreatedAt = currency.CreatedAt,
            UpdatedAt = currency.UpdatedAt
        }).ToList();

        return Ok(ApiResponse<List<CurrencyDto>>.Success(currencyDtos, "Валюти отримано"));
    }

    // GET: api/Currency/5
    [HttpGet("{id:int}", Name = "currency")]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> GetCurrency(int id)
    {
        var currency = await unitOfWork.Currencies.GetByIdAsync(id);

        if (currency == null)
        {
            return NotFound(ApiResponse<CurrencyDto>.Fail(new[] { "Валюту не знайдено" }, "Not Found"));
        }
        await unitOfWork.CompleteAsync();

        var currencyDto = new CurrencyDto
        {
            Id = currency.Id,
            Name = currency.Name,
            Symbol = currency.Symbol,
            CreatedAt = currency.CreatedAt,
            UpdatedAt = currency.UpdatedAt
        };

        return Ok(ApiResponse<CurrencyDto>.Success(currencyDto, "Валюта отримана"));
    }

    // POST: api/Currency
    [HttpPost(Name = "createCurrency")]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> CreateCurrency(CreateCurrencyDto createCurrencyDto)
    {
        var currency = new Currency
        {
            Name = createCurrencyDto.Name,
            Symbol = createCurrencyDto.Symbol,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var entityEntry = await unitOfWork.Currencies.AddAsync(currency);
        await unitOfWork.CompleteAsync();

        var createdCurrency = entityEntry.Entity;
        var currencyDto = new CurrencyDto
        {
            Id = createdCurrency.Id,
            Name = createdCurrency.Name,
            Symbol = createdCurrency.Symbol,
            CreatedAt = createdCurrency.CreatedAt,
            UpdatedAt = createdCurrency.UpdatedAt
        };

        return CreatedAtRoute("currency", new { id = currencyDto.Id }, ApiResponse<CurrencyDto>.Success(currencyDto, "Валюта створена"));
    }

    // PUT: api/Currency/5
    [HttpPut("{id:int}", Name = "updateCurrency")]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> UpdateCurrency(int id, UpdateCurrencyDto updateCurrencyDto)
    {
        var currency = await unitOfWork.Currencies.GetByIdAsync(id);
        if (currency == null)
        {
            return NotFound(ApiResponse<CurrencyDto>.Fail(new[] { "Валюту не знайдено" }, "Not Found"));
        }

        currency.Name = updateCurrencyDto.Name;
        currency.Symbol = updateCurrencyDto.Symbol;
        currency.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Currencies.Update(currency);
        await unitOfWork.CompleteAsync();

        var updatedCurrencyDto = new CurrencyDto
        {
            Id = currency.Id,
            Name = currency.Name,
            Symbol = currency.Symbol,
            CreatedAt = currency.CreatedAt,
            UpdatedAt = currency.UpdatedAt
        };

        return Ok(ApiResponse<CurrencyDto>.Success(updatedCurrencyDto, "Валюту оновлено"));
    }

    // DELETE: api/Currency/5
    [HttpDelete("{id:int}", Name = "deleteCurrency")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCurrency(int id)
    {
        var currency = await unitOfWork.Currencies.GetByIdAsync(id);
        if (currency == null)
        {
            return NotFound(ApiResponse<object>.Fail(new[] { "Валюту не знайдено" }, "Not Found"));
        }

        unitOfWork.Currencies.Delete(currency);
        await unitOfWork.CompleteAsync();

        return Ok(ApiResponse<object>.Success(null, "Валюту видалено"));
    }
}
