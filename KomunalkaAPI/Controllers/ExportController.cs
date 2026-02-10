using System.Security.Claims;
using Asp.Versioning;
using KomunalkaAPI.DTO;
using KomunalkaAPI.DTO.Export;
using KomunalkaAPI.Services.Export;
using KomunalkaAPI.Services.MeterReading;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KomunalkaAPI.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/export")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly IMeterReadingService _meterReadingService;
    private readonly IExportService _exportService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(
        IMeterReadingService meterReadingService,
        IExportService exportService,
        ILogger<ExportController> logger)
    {
        _meterReadingService = meterReadingService;
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// Export meter readings for the authenticated user
    /// </summary>
    [HttpPost("meter-readings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportMeterReadings([FromBody] ExportRequestDto request)
    {
        var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdString, out var userId))
        {
            return Unauthorized();
        }

        var result = await _meterReadingService.GetReadingsForExportAsync(userId, request);

        if (!result.Success)
        {
            return BadRequest(new { error = result.Errors?.FirstOrDefault() ?? "Unknown error" });
        }

        if (result.Data == null)
        {
            // Should not happen if Success is true, but handle it anyway
             return BadRequest(new { error = "No data found for export" });
        }

        byte[] fileBytes;
        string contentType;
        string fileName;

        try
        {
            if (request.Format == ExportFormat.Csv)
            {
                fileBytes = _exportService.GenerateCsv(result.Data);
                contentType = "text/csv";
                fileName = $"meter_readings_{DateTime.Now:yyyyMMddHHmmss}.csv";
            }
            else
            {
                fileBytes = _exportService.GeneratePdf(result.Data);
                contentType = "application/pdf";
                fileName = $"meter_readings_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            }

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating export file");
            return StatusCode(500, new { error = "An error occurred while generating the export file." });
        }
    }
}
