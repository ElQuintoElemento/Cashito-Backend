using CashitoBackend.IAM.Application.Internal.OutboundServices;
using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.IAM.Domain.Model.Commands;
using CashitoBackend.IAM.Domain.Repositories;
using CashitoBackend.IAM.Domain.Services;
using CashitoBackend.Shared.Domain.Exceptions;
using CashitoBackend.Shared.Domain.Repositories;
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
            throw new BadRequestException("Invalid username or password");

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
        if (string.IsNullOrWhiteSpace(command.Username) || command.Username.Length < 4)
            throw new BadRequestException("Username must be at least 4 characters long");

        if (string.IsNullOrWhiteSpace(command.Password) || command.Password.Length < 8)
            throw new BadRequestException("Password must be at least 8 characters long");

        if (userRepository.ExistsByUsername(command.Username))
            throw new BadRequestException("Username already exists");

        var emailVo = new EmailAddress(command.Email);
        if (await userRepository.ExistsByEmailAsync(emailVo))
            throw new BadRequestException("Email address already exists");

        var passwordHash = hashingService.HashPassword(command.Password);

        var user = new User(command.Username, passwordHash)
            .UpdatePersonalInfo(command.FirstName, command.LastName, emailVo);
        

        await userRepository.AddAsync(user);
        await unitOfWork.CompleteAsync();

        return user;
    }
    

    public async Task<User> Handle(UpdateUserCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
                   ?? throw new NotFoundException("User not found");

        user.UpdatePersonalInfo(
            command.FirstName,
            command.LastName,
            new EmailAddress(command.Email)
        );

        await unitOfWork.CompleteAsync();

        return user;
    }
    
    
    public async Task<User> Handle(ChangePasswordCommand command)
    {
        var user = await userRepository.FindByIdAsync(command.UserId)
                   ?? throw new NotFoundException("User not found");

        if (!hashingService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            throw new BadRequestException("Current password is incorrect");

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < 8)
            throw new BadRequestException("New password must be at least 8 characters long");

        var newHash = hashingService.HashPassword(command.NewPassword);

        user.UpdatePasswordHash(newHash);

        await unitOfWork.CompleteAsync();

        return user;
    }
}