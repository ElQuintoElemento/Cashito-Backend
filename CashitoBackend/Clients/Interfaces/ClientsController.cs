using CashitoBackend.Clients.Domain.Model.Commands;
using CashitoBackend.Clients.Domain.Model.Queries;
using CashitoBackend.Clients.Domain.Services;
using CashitoBackend.Clients.Interfaces.REST.Resources;
using CashitoBackend.Clients.Interfaces.REST.Transform;
using CashitoBackend.IAM.Infrastructure.Pipeline.Middleware.Attributes;
using CashitoBackend.Shared.Infrastructure.Interfaces.ASP.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace CashitoBackend.Clients.Interfaces;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientsController : ControllerBase
{
    private readonly IClientCommandService _commandService;
    private readonly IClientQueryService _queryService;

    public ClientsController(
        IClientCommandService commandService,
        IClientQueryService queryService)
    {
        _commandService = commandService;
        _queryService = queryService;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientResource resource)
    {
        var userId = User.GetUserId();

        var command = CreateClientCommandFromResourceAssembler
            .ToCommandFromResource(resource);

        var result = await _commandService.Handle(command, userId);

        var response = ClientResourceFromEntityAssembler
            .ToResourceFromEntity(result);

        return Ok(response);
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllClients()
    {
        var userId = User.GetUserId();

        var query = new GetAllClientsQuery(userId);

        var result = await _queryService.Handle(query);

        var response = result
            .Select(ClientResourceFromEntityAssembler.ToResourceFromEntity);

        return Ok(response);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetClientById(int id)
    {
        var userId = User.GetUserId();

        var query = new GetClientByIdQuery(id, userId);

        var result = await _queryService.Handle(query);

        if (result == null)
            return NotFound();

        return Ok(ClientResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateClient(int id, [FromBody] UpdateClientResource resource)
    {
        var userId = User.GetUserId();

        var command = UpdateClientCommandFromResourceAssembler
            .ToCommandFromResource(id, resource);

        var result = await _commandService.Handle(command, userId);

        if (result == null)
            return NotFound();

        return Ok(ClientResourceFromEntityAssembler.ToResourceFromEntity(result));
    }
    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var userId = User.GetUserId();

        var command = new DeleteClientCommand(id);

        var result = await _commandService.Handle(command, userId);

        if (!result)
            return NotFound();

        return Ok(new { message = "Client deleted successfully" });
    }
}