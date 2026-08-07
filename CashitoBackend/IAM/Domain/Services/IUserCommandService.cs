using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.IAM.Domain.Model.Commands;

namespace CashitoBackend.IAM.Domain.Services;

/**
 * <summary>
 *     The user command service
 * </summary>
 * <remarks>
 *     This interface is used to handle user commands
 * </remarks>
 */
public interface IUserCommandService
{
    /**
        * <summary>
        *     Handle sign in command
        * </summary>
        * <param name="command">The sign in command</param>
        * <returns>The authenticated user and the JWT token</returns>
        */
    Task<(User user, string token)> Handle(SignInCommand command);

    /**
        * <summary>
        *     Handle sign up command
        * </summary>
        * <param name="command">The sign up command</param>
        * <returns>A confirmation message on successful creation.</returns>
        */
    Task<User> Handle(SignUpCommand command);

    /// <summary>
    ///     Handle update user command
    /// </summary>
    Task<User> Handle(UpdateUserCommand command);
    
    Task<User> Handle(ChangePasswordCommand command);
}