using CashitoBackend.Credits.Domain.Model.Queries;
using CashitoBackend.Credits.Domain.Services;
using CashitoBackend.Credits.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace CashitoBackend.Credits.Interfaces.REST;

[ApiController]
[Route("api/public/credits")]
public class PublicCreditsController : ControllerBase
{
    private readonly ICreditQueryService _queryService;
    private readonly ICreditCommandService _commandService;

    public PublicCreditsController(
        ICreditQueryService queryService,
        ICreditCommandService commandService)
    {
        _queryService = queryService;
        _commandService = commandService;
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPublic(int id, [FromQuery] string token)
    {
        var credit = await _queryService.Handle(new GetCreditByIdQuery(id, 0));

        if (credit == null || credit.PublicToken != token)
            return Unauthorized();

        return Ok(CreditResourceFromEntityAssembler.ToResourceFromEntity(credit));
    }

    [HttpGet("{id:int}/schedule")]
    public async Task<IActionResult> GetSchedule(int id, [FromQuery] string token)
    {
        var credit = await _queryService.Handle(new GetCreditByIdQuery(id, 0));

        if (credit == null || credit.PublicToken != token)
            return Unauthorized();

        return Ok(credit.Schedule.Select(
            InstallmentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpPut("{id:int}/installments/{number}/pay")]
    public async Task<IActionResult> Pay(int id, int number, [FromQuery] string token)
    {
        var credit = await _queryService.Handle(new GetCreditByIdQuery(id, 0));

        if (credit == null || credit.PublicToken != token)
            return Unauthorized();

        var result = await _commandService.PayInstallment(id, number, credit.UserId);

        if (!result) return NotFound();

        return NoContent();
    }
}