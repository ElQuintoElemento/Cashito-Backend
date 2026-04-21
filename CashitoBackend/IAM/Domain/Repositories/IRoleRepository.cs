using CashitoBackend.IAM.Domain.Model.Entities;
using CashitoBackend.Shared.Domain.Repositories;

namespace CashitoBackend.IAM.Domain.Repositories;

public interface IRoleRepository : IBaseRepository<Role>
{
    Task<Role?> FindByNameAsync(string name);
} 