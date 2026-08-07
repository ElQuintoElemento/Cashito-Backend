using System.Net.Mime;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.IAM.Interfaces.REST.Resources;
using CashitoBackend.IAM.Interfaces.REST.Transform;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CashitoBackend.IAM.Interfaces.REST;

[ApiController]
[Route("api/v1/auth")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Authentication endpoints")]
public class AuthenticationController(IUserCommandService userCommandService, ILogger<AuthenticationController> logger) : ControllerBase
{
    /**
     * <summary>
     *     Sign in endpoint. It allows authenticating a user
     * </summary>
     * <param name="signInResource">The sign-in resource containing username and password.</param>
     * <returns>The authenticated user resource, including a JWT token</returns>
     */
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Sign in",
        Description = "Sign in a user",
        OperationId = "SignIn")]
    [SwaggerResponse(StatusCodes.Status200OK, "The user was authenticated", typeof(AuthenticatedUserResource))]
    public async Task<IActionResult> SignIn([FromBody] SignInResource signInResource)
    {
        try
        {
            var signInCommand = SignInCommandFromResourceAssembler.ToCommandFromResource(signInResource);
            var authenticatedUser = await userCommandService.Handle(signInCommand);
            var resource = AuthenticatedUserResourceFromEntityAssembler.ToResourceFromEntity(
                authenticatedUser.user,
                authenticatedUser.token);
            return Ok(resource);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Invalid sign-in for user {User}", signInResource.Username);
            return BadRequest(new { error = ex.Message, path = HttpContext.Request.Path });
        }
    }

    /**
     * <summary>
     *     Sign up endpoint. It allows creating a new user
     * </summary>
     * <param name="signUpResource">The sign-up resource containing username and password.</param>
     * <returns>A confirmation message on successful creation.</returns>
     */
    [HttpPost("sign-up")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Sign-up",
        Description = "Sign up a new user",
        OperationId = "SignUp")]
    [SwaggerResponse(StatusCodes.Status201Created, "The user was created successfully")]
    public async Task<IActionResult> SignUp([FromBody] SignUpResource signUpResource)
    {
        try
        {
            logger.LogInformation("Processing sign-up for user {Username}", signUpResource.Username);
            var signUpCommand = SignUpCommandFromResourceAssembler.ToCommandFromResource(signUpResource);
            var user = await userCommandService.Handle(signUpCommand);
            var userResource = UserResourceFromEntityAssembler.ToResourceFromEntity(user);
            return Created($"/api/v1/users/{user.Id}", userResource);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during sign-up");
            return BadRequest(new { error = ex.Message, path = HttpContext.Request.Path });
        }
    }
    
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        var userId = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? User.FindFirst("sub")?.Value;
        var username = User.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value ?? User.FindFirst("unique_name")?.Value;

        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        return Ok(new
        {
            id = userId,
            username = username
        });
    }
}