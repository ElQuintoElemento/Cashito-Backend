using System.Linq;
using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Domain.Repositories;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.Shared.Domain.Repositories;
using CashitoBackend.IAM.Domain.Model.Entities;
using CashitoBackend.IAM.Domain.Model.ValueObjects;
using CashitoBackend.Shared.Domain.Model.ValueObjects;

namespace CashitoBackend.IAM.Application.Internal.CommandServices;

/**
 * <summary>
 *     The user command service
 * </summary>
 * <remarks>
 *     This class is used to handle user commands
 * </remarks>
 */
public class UserCommandService(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork
) : IUserCommandService
{
    /**
     * <summary>
     *     Handle sign in command
     * </summary>
     * <param name="command">The sign in command</param>
     * <returns>The authenticated user and the JWT token</returns>
     */
    public async Task<(User user, string token)> Handle(SignInCommand command)
    {
        var user = await userRepository.FindByUsernameAsync(command.Username);

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            throw new Exception("Invalid username or password");

        var token = tokenService.GenerateToken(user);

        return (user, token);
    }

    /**
     * <summary>
     *     Handle sign up command
     * </summary>
     * <param name="command">The sign up command</param>
     * <returns>A confirmation message on successful creation.</returns>
     */
    public async Task<User> Handle(SignUpCommand command)
    {
        if (userRepository.ExistsByUsername(command.Username))
            throw new Exception("Username already exists");

        var passwordHash = hashingService.HashPassword(command.Password);

        var user = new User(command.Username, passwordHash)
            .UpdatePersonalInfo(command.FirstName, command.LastName, new EmailAddress(command.Email));

        var role = await roleRepository.FindByNameAsync(nameof(Roles.User));

        if (role == null)
        {
            role = new Role(Roles.User);
            await roleRepository.AddAsync(role);
        }

        user.AddRole(role);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();

        return user;
    }

    public async Task<User> Handle(CreateUserCommand command)
    {
        if (userRepository.ExistsByUsername(command.Username))
            throw new Exception("Username already exists");

        var user = new User(
            command.Username,
            hashingService.HashPassword(command.Password)
        ).UpdatePersonalInfo(
            command.FirstName,
            command.LastName,
            new EmailAddress(command.Email)
        );

        var roles = new List<Role>();

        foreach (var roleName in command.Roles)
        {
            var role = await roleRepository.FindByNameAsync(roleName);

            if (role == null)
            {
                role = new Role(Enum.Parse<Roles>(roleName, true));
                await roleRepository.AddAsync(role);
            }

            roles.Add(role);
        }

        user.AddRoles(roles);

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();

        return user;
    }

    public async Task<User> Handle(UpdateUserCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
                   ?? throw new Exception("User not found");

        user.UpdatePersonalInfo(
            command.FirstName,
            command.LastName,
            new EmailAddress(command.Email)
        );

        await unitOfWork.CompleteAsync();

        return user;
    }

    public async Task<User> Handle(DeleteUserCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
                   ?? throw new Exception("User not found");

        user.Deactivate();

        await unitOfWork.CompleteAsync();

        return user;
    }
    
    public async Task<User> Handle(ChangePasswordCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
                   ?? throw new Exception("User not found");

        if (!hashingService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            throw new Exception("Current password is incorrect");

        var newHash = hashingService.HashPassword(command.NewPassword);

        user.UpdatePasswordHash(newHash);

        await unitOfWork.CompleteAsync();

        return user;
    }
}