using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Linq.Expressions;

namespace Data.Interfaces
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        EntityEntry<TEntity> Delete(TEntity entity);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> filter);
        Task<List<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>>? filter = null,
                                               string[]? includeProperties = null,
                                               Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null);
        Task<TEntity?> GetAsync(Guid? id, string[]? includeProperties = null);
        Task<TEntity> InsertAsync(TEntity entity);
        bool Update(TEntity entity);
    }
}
