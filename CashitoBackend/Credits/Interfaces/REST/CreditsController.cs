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

    // =========================
    // 🔥 SIMULACIÓN
    // =========================
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

    // =========================
    // 💾 CREAR CRÉDITO
    // =========================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCreditResource resource)
    {
        var userId = User.GetUserId();

        var command = CreateCreditCommandFromResourceAssembler
            .ToCommandFromResource(resource);

        var result = await _commandService.Handle(command, userId);

        return Ok(CreditResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    // =========================
    // 📄 LISTAR
    // =========================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = User.GetUserId();

        var result = await _queryService.Handle(new GetAllCreditsQuery(userId));

        return Ok(result.Select(CreditResourceFromEntityAssembler.ToResourceFromEntity));
    }

    // =========================
    // 📄 DETALLE
    // =========================
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = User.GetUserId();

        var result = await _queryService.Handle(new GetCreditByIdQuery(id, userId));

        if (result == null)
            return NotFound();

        return Ok(CreditResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    // =========================
    // 📊 CRONOGRAMA
    // =========================
    [HttpGet("{id:int}/schedule")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        var userId = User.GetUserId();

        var result = await _queryService.Handle(new GetCreditScheduleQuery(id, userId));

        return Ok(result.Select(InstallmentResourceFromEntityAssembler.ToResourceFromEntity));
    }

    // =========================
    // 🔁 ESTADOS (CORE)
    // =========================

    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var userId = User.GetUserId();

        var result = await _commandService.Approve(id, userId);

        if (!result) return NotFound();

        return NoContent();
    }

    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var userId = User.GetUserId();

        var result = await _commandService.Activate(id, userId);

        if (!result) return NotFound();

        return NoContent();
    }

    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var userId = User.GetUserId();

        var result = await _commandService.Reject(id, userId);

        if (!result) return NotFound();

        return NoContent();
    }

    [HttpPut("{id:int}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        var userId = User.GetUserId();

        var result = await _commandService.Complete(id, userId);

        if (!result) return NotFound();

        return NoContent();
    }

    // =========================
    // 💸 PAGAR CUOTA (ASESOR)
    // =========================
    [HttpPut("{id:int}/installments/{number}/pay")]
    public async Task<IActionResult> PayInstallment(int id, int number)
    {
        var userId = User.GetUserId();

        var result = await _commandService.PayInstallment(id, number, userId);

        if (!result) return NotFound();

        return NoContent();
    }
}