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

    public PublicCreditsController(ICreditPublicService publicService)
    {
        _publicService = publicService;
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
