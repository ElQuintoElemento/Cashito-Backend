using System.Net.Mime;
using CashitoBackend.IAM.Domain.Model.Queries;
using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.IAM.Interfaces.REST.Resources;
using CashitoBackend.IAM.Interfaces.REST.Transform;
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
    // CREATE USER (PUBLIC / SIGNUP ADMIN STYLE)
    // =========================
    [HttpPost]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Create user",
        OperationId = "CreateUser")]
    public async Task<IActionResult> Create([FromBody] CreateUserResource resource)
    {
        var command = CreateUserCommandFromResourceAssembler.ToCommandFromResource(resource);

        var user = await userCommandService.Handle(command);

        return Created(
            $"/api/v1/users/{user.Id}",
            UserResourceFromEntityAssembler.ToResourceFromEntity(user)
        );
    }

    // =========================
    // UPDATE USER (ADMIN ONLY VIA ROLE)
    // =========================
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Update user",
        OperationId = "UpdateUser")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserResource resource)
    {
        var command = UpdateUserCommandFromResourceAssembler.ToCommandFromResource(id, resource);

        var user = await userCommandService.Handle(command);

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }
    
    // =========================
    // CHANGE PASSWORD
    // =========================
    [HttpPost("{id:int}/change-password")]
    [Authorize]
    [SwaggerOperation(
        Summary = "Change user password",
        OperationId = "ChangePassword")]
    public async Task<IActionResult> ChangePassword(
        int id,
        [FromBody] ChangePasswordResource resource)
    {
        var command = ChangePasswordCommandFromResourceAssembler.ToCommandFromResource(id, resource);

        var result = await userCommandService.Handle(command);

        return Ok(new
        {
            message = "Password updated successfully",
            userId = result.Id
        });
    }

    // =========================
    // DELETE USER (ADMIN ONLY VIA ROLE)
    // =========================
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [SwaggerOperation(
        Summary = "Delete user",
        OperationId = "DeleteUser")]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await userCommandService.Handle(new DeleteUserCommand(id));

        return Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(user));
    }
}