using System.Linq.Expressions;

namespace CashitoBackend.Shared.Domain.Repositories;

public interface IBaseRepository<TEntity>
{
    Task AddAsync(TEntity entity);
    
    Task<TEntity?> FindByIdAsync(long id);

    void Update(TEntity entity);
    
    void Remove(TEntity entity);
    
    Task<IEnumerable<TEntity>> ListAsync();
    
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
}