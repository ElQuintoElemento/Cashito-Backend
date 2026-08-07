using CashitoBackend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Extensions;
using CashitoBackend.Vehicles.Domain.Model.Commands;
using CashitoBackend.Vehicles.Domain.Model.Queries;
using CashitoBackend.Vehicles.Domain.Services;
using CashitoBackend.Vehicles.Interfaces.REST.Resources;
using CashitoBackend.Vehicles.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Mvc;

namespace CashitoBackend.Vehicles.Interfaces.REST;


[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleCommandService _commandService;
    private readonly IVehicleQueryService _queryService;

    public VehiclesController(
        IVehicleCommandService commandService,
        IVehicleQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleResource resource)
    {
        var userId = User.GetUserId();

        var command = CreateVehicleCommandFromResourceAssembler
            .ToCommandFromResource(resource);

        var result = await _commandService.Handle(command, userId);

        var response = VehicleResourceFromEntityAssembler
            .ToResourceFromEntity(result);

        return Ok(response);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllVehicles()
    {
        var userId = User.GetUserId();

        var query = new GetAllVehiclesQuery(userId);

        var result = await _queryService.Handle(query);

        var response = result
            .Select(VehicleResourceFromEntityAssembler.ToResourceFromEntity);

        return Ok(response);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetVehicleById(int id)
    {
        var userId = User.GetUserId();

        var query = new GetVehicleByIdQuery(id, userId);

        var result = await _queryService.Handle(query);

        if (result == null)
            return NotFound();

        return Ok(VehicleResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleResource resource)
    {
        var userId = User.GetUserId();

        var command = UpdateVehicleCommandFromResourceAssembler
            .ToCommandFromResource(id, resource);

        var result = await _commandService.Handle(command, userId);

        if (result == null)
            return NotFound();

        return Ok(VehicleResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteVehicle(int id)
    {
        var userId = User.GetUserId();

        var command = new DeleteVehicleCommand(id);

        var result = await _commandService.Handle(command, userId);

        if (!result)
            return NotFound();

        return Ok(new { message = "Vehicle deleted successfully" });
    }
}