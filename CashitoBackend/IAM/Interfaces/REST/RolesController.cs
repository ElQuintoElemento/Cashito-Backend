using System.Net.Mime;
using CashitoBackend.IAM.Domain.Model.Queries;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.IAM.Interfaces.REST.Transform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CashitoBackend.IAM.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Roles endpoints")] 
public class RolesController(IRoleQueryService roleQueryService) : ControllerBase
{
    [HttpGet]
    [SwaggerOperation(Summary = "Get all roles", OperationId = "GetRoles")]
    public async Task<IActionResult> GetAll()
    {
        var roles = await roleQueryService.Handle(new GetAllRolesQuery());
        var resources = roles.Select(RoleResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }
} 