using CashitoBackend.Credits.Domain.Model.Queries;
using CashitoBackend.Credits.Domain.Services;
using CashitoBackend.Credits.Interfaces.REST.Resources;
using CashitoBackend.Credits.Interfaces.REST.Transform;
using CashitoBackend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CashitoBackend.Credits.Interfaces.REST;

[ApiController]
[Route("api/credits")]
[Authorize]
public class CreditsController : ControllerBase
{
    private readonly ICreditCommandService _commandService;
    private readonly ICreditQueryService _queryService;

    public CreditsController(
        ICreditCommandService commandService,
        ICreditQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }
    
    [HttpPost("simulate")]
    public async Task<IActionResult> Simulate([FromBody] SimulateCreditResource resource)
    {
        var userId = User.GetUserId();

        var command = SimulateCreditCommandFromResourceAssembler
            .ToCommandFromResource(resource);

        var result = await _commandService.Handle(command, userId);

        var response = new SimulationResponseResource(
            result.Cuota,
            result.Tcea,
            result.Van,
            result.Tir,
            result.Installments.Select(InstallmentResourceFromEntityAssembler.ToResourceFromEntity)
        );

        return Ok(response);
    }
    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCreditResource resource)
    {
        var userId = User.GetUserId();

        var command = CreateCreditCommandFromResourceAssembler
            .ToCommandFromResource(resource);

        var result = await _commandService.Handle(command, userId);

        var response = CreditResourceFromEntityAssembler
            .ToResourceFromEntity(result);

        return Ok(response);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();

        var query = new GetAllCreditsQuery(userId);

        var result = await _queryService.Handle(query);

        var response = result
            .Select(CreditResourceFromEntityAssembler.ToResourceFromEntity);

        return Ok(response);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.GetUserId();

        var query = new GetCreditByIdQuery(id, userId);

        var result = await _queryService.Handle(query);

        if (result == null)
            return NotFound();

        return Ok(CreditResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
    
    [HttpGet("{id:int}/schedule")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var userId = User.GetUserId();

        var query = new GetCreditScheduleQuery(id, userId);

        var result = await _queryService.Handle(query);

        var response = result
            .Select(InstallmentResourceFromEntityAssembler.ToResourceFromEntity);

        return Ok(response);
    }
}