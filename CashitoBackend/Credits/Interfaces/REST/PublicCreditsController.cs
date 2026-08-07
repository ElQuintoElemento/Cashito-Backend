using CashitoBackend.Credits.Domain.Services;
using CashitoBackend.Credits.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CashitoBackend.Credits.Interfaces.REST;

[ApiController]
[Route("api/public/credits")]
[AllowAnonymous]
public class PublicCreditsController : ControllerBase
{
    private readonly ICreditPublicService _publicService;
    private readonly ICreditExportService _exportService;

    public PublicCreditsController(
        ICreditPublicService publicService,
        ICreditExportService exportService)
    {
        _publicService = publicService;
        _exportService = exportService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPublic(int id, [FromQuery] string token)
    {
        var credit = await _publicService.GetCreditAsync(id, token);

        if (credit == null)
            return Unauthorized();

        return Ok(CreditResourceFromEntityAssembler.ToResourceFromEntity(credit));
    }

    [HttpGet("{id:int}/schedule")]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] string token)
    {
        var schedule = await _publicService.GetScheduleAsync(id, token);

        if (schedule == null)
            return Unauthorized();

        return Ok(schedule.Select(InstallmentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}/pdf")]
    public async Task<IActionResult> ExportPdf(int id, [FromQuery] string token)
    {
        try
        {
            var pdfBytes = await _exportService.GeneratePdfPublicAsync(id, token);
            return File(pdfBytes, "application/pdf", $"Credit-{id}.pdf");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:int}/excel")]
    public async Task<IActionResult> ExportExcel(int id, [FromQuery] string token)
    {
        try
        {
            var excelBytes = await _exportService.GenerateExcelPublicAsync(id, token);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Credit-{id}.xlsx");
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/installments/{number:int}/pay")]
    public async Task<IActionResult> Pay(int id, int number, [FromQuery] string token)
    {
        if (await _publicService.GetCreditAsync(id, token) == null)
            return Unauthorized();

        if (!await _publicService.PayInstallmentAsync(id, number, token))
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromQuery] string token)
    {
        if (await _publicService.GetCreditAsync(id, token) == null)
            return Unauthorized();

        if (!await _publicService.ApproveAsync(id, token))
            return NotFound();

        return NoContent();
    }

    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromQuery] string token)
    {
        if (await _publicService.GetCreditAsync(id, token) == null)
            return Unauthorized();

        if (!await _publicService.RejectAsync(id, token))
            return NotFound();

        return NoContent();
    }
}
