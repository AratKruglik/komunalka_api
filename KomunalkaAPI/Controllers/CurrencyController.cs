using KomunalkaAPI.DTO;
using KomunalkaAPI.Services.Currency;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CurrencyController(ICurrencyService currencyService) : ControllerBase
{
    [HttpGet(Name = "currencies")]
    public async Task<ActionResult<ApiResponse<List<CurrencyDto>>>> GetCurrencies()
    {
        var list = await currencyService.GetAllAsync();
        if (!list.Any())
        {
            return NotFound(ApiResponse<List<CurrencyDto>>.Fail(new[] { "Валюти не знайдено" }, "Not Found"));
        }
        return Ok(ApiResponse<List<CurrencyDto>>.Success(list.ToList(), "Валюти отримано"));
    }

    [HttpGet("{id:int}", Name = "currency")]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> GetCurrency(int id)
    {
        var result = await currencyService.GetByIdAsync(id);
        if (result.NotFound)
        {
            return NotFound(ApiResponse<CurrencyDto>.Fail(new[] { "Валюту не знайдено" }, "Not Found"));
        }
        return Ok(ApiResponse<CurrencyDto>.Success(result.Data!, "Валюта отримана"));
    }

    [HttpPost(Name = "createCurrency")]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> CreateCurrency(CreateCurrencyDto createCurrencyDto)
    {
        var currencyDto = await currencyService.CreateAsync(createCurrencyDto);
        return CreatedAtRoute("currency", new { id = currencyDto.Id }, ApiResponse<CurrencyDto>.Success(currencyDto, "Валюта створена"));
    }

    [HttpPut("{id:int}", Name = "updateCurrency")]
    public async Task<ActionResult<ApiResponse<CurrencyDto>>> UpdateCurrency(int id, UpdateCurrencyDto updateCurrencyDto)
    {
        var result = await currencyService.UpdateAsync(id, updateCurrencyDto);
        if (result.NotFound)
        {
            return NotFound(ApiResponse<CurrencyDto>.Fail(new[] { "Валюту не знайдено" }, "Not Found"));
        }
        return Ok(ApiResponse<CurrencyDto>.Success(result.Data!, "Валюту оновлено"));
    }

    [HttpDelete("{id:int}", Name = "deleteCurrency")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteCurrency(int id)
    {
        var result = await currencyService.DeleteAsync(id);
        if (result.NotFound)
        {
            return NotFound(ApiResponse<object>.Fail(new[] { "Валюту не знайдено" }, "Not Found"));
        }
        return Ok(ApiResponse<object>.Success(null, "Валюту видалено"));
    }
}
