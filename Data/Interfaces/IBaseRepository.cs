using Data.Models;
using System.Linq.Expressions;

namespace Data.Interfaces
{
    public interface IBaseRepository<TEntity, TModel>
        where TEntity : class
        where TModel : class
    {
        Task BeginTransactionAsync();
        Task RollbackTransactionAsync();
        Task CommitTransactionAsync();
        Task<ApiResponse<bool>> AddAsync(TEntity entity);
        Task<ApiResponse<bool>> DeleteAsync(TEntity entity);
        Task<ApiResponse<bool>> DeleteByIdAsync(string id);
        Task<ApiResponse<bool>> UpdateAsync(TEntity entity);
        Task<ApiResponse<TModel>> GetAsync(Expression<Func<TEntity, bool>> where, params Expression<Func<TEntity, object>>[] includes);
        Task<ApiResponse<IEnumerable<TModel>>> GetAllAsync(bool orderByDescending = false, Expression<Func<TEntity, object>>? sortBy = null, Expression<Func<TEntity, bool>>? where = null, params Expression<Func<TEntity, object>>[] includes);
        Task<ApiResponse<IEnumerable<TSelect>>> GetAllAsync<TSelect>(Expression<Func<TEntity, TSelect>> selector, bool orderByDescending = false, Expression<Func<TEntity, object>>? sortBy = null, Expression<Func<TEntity, bool>>? where = null, params Expression<Func<TEntity, object>>[] includes);
        Task<ApiResponse<bool>> ExistsAsync(Expression<Func<TEntity, bool>> findBy);
        Task<TEntity?> FindByIdAsync(string id);
    }
}