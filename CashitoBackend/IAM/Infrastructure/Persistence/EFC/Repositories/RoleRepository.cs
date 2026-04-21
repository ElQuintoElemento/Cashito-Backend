using CashitoBackend.IAM.Domain.Model.Entities;
using CashitoBackend.IAM.Domain.Repositories;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Configuration;
using CashitoBackend.Shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CashitoBackend.IAM.Infrastructure.Persistence.EFC.Repositories;

public class RoleRepository(AppDbContext context)
    : BaseRepository<Role>(context), IRoleRepository
{
    public async Task<Role?> FindByNameAsync(string name)
    {
        return await context.Set<Role>()
            .FirstOrDefaultAsync(r => r.Name.ToString() == name);
    }
} 