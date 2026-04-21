using CashitoBackend.IAM.Domain.Model.Aggregates;
using CashitoBackend.IAM.Domain.Repositories;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.IAM.Infrastructure.Persistence.EFC.Repositories;

/**
 * <summary>
 *     The user repository
 * </summary>
 * <remarks>
 *     This repository is used to manage users
 * </remarks>
 */
public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    /**
     * <summary>
     *     Find a user by username
     * </summary>
     * <param name="username">The username to search</param>
     * <returns>The user</returns>
     */
    public async Task<User?> FindByUsernameAsync(string username)
    {
        return await Context.Set<User>()
                     .IgnoreQueryFilters()
                     .Include(u => u.Roles)
                     .FirstOrDefaultAsync(user => user.Username == username);
    }

    /**
     * <summary>
     *     Check if a user exists by username
     * </summary>
     * <param name="username">The username to search</param>
     * <returns>True if the user exists, false otherwise</returns>
     */
    public bool ExistsByUsername(string username)
    {
        return Context.Set<User>()
                     .IgnoreQueryFilters()
                     .Any(user => user.Username == username);
    }

    // Aseguramos que el usuario se obtenga con sus roles para la autorización
    public new async Task<User?> FindByIdAsync(long id)
    {
        return await Context.Set<User>()
                     .Include(u => u.Roles)
                     .FirstOrDefaultAsync(u => u.Id == id);
    }

    // Listado con roles (útil para endpoints de listado de usuarios)
    public new async Task<IEnumerable<User>> ListAsync()
    {
        return await Context.Set<User>()
            .Include(u => u.Roles)
            .Where(u => u.Active) // No compares con 1 si es bool
            .ToListAsync();
    }
    
}