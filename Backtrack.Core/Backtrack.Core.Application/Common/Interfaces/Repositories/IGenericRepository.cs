using System.Threading.Tasks;

namespace Backtrack.Core.Application.Common.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class
    {
        Task<TEntity> CreateAsync(TEntity entity);
        Task<TEntity?> GetByIdAsync(TKey id);
        bool Update(TEntity entity);
        Task<bool> DeleteAsync(TKey id);
        Task<TEntity> UpsertAsync(TKey id, TEntity entity);
        Task SaveChangesAsync();
    }
}
