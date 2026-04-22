using System.Net.Mime;
using CashitoBackend.IAM.Domain.Model.Queries;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.IAM.Interfaces.REST.Resources;
using CashitoBackend.IAM.Interfaces.REST.Transform;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CashitoBackend.IAM.Interfaces.REST;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available User endpoints")]
public class UsersController(
    IUserQueryService userQueryService,
    IUserCommandService userCommandService
) : ControllerBase
{
    // =========================
    // GET USER BY ID
    // =========================
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Get a user by its id",
        Description = "Get a user by its id",
        OperationId = "GetUserById")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was found", typeof(UserResource))]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await userQueryService.Handle(new GetUserByIdQuery(id));

        if (user == null)
            return NotFound();

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }

    // =========================
    // GET ALL USERS
    // =========================
    [HttpGet]
    [SwaggerOperation(
        Summary = "Get all users",
        Description = "Get all users",
        OperationId = "GetAllUsers")]
    [SwaggerResponse(StatusCodes.Status200OK, "The users were found", typeof(IEnumerable<UserResource>))]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await userQueryService.Handle(new GetAllUsersQuery());

        var result = users
            .Select(UserResourceFromEntityAssembler.ToResourceFromEntity);

        return Ok(result);
    }

    // =========================
    // UPDATE USER
    // =========================
    [HttpPut("{id:int}")]
    [SwaggerOperation(
        Summary = "Update user",
        OperationId = "UpdateUser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserResource resource)
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var currentUserId))
            return Unauthorized();
        
        if (currentUserId != id)
            return Forbid();
        
        var command = UpdateUserCommandFromResourceAssembler.ToCommandFromResource(id, resource);

        var user = await userCommandService.Handle(command);

        if (user == null)
            return NotFound();

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }
    
    // =========================
    // CHANGE PASSWORD
    // =========================
    [HttpPost("{id:int}/change-password")]
    [SwaggerOperation(
        Summary = "Change user password",
        OperationId = "ChangePassword")]
    public async Task<IActionResult> ChangePassword(
        int id,
        [FromBody] ChangePasswordResource resource)
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(sub) || !int.TryParse(sub, out var currentUserId))
            return Unauthorized();
        
        if (currentUserId != id)
            return Forbid();
        
        var command = ChangePasswordCommandFromResourceAssembler.ToCommandFromResource(id, resource);

        var result = await userCommandService.Handle(command);

        return Ok(new
        {
            message = "Password updated successfully",
            userId = result.Id
        });
    }
}