using System.Threading.Tasks;

namespace Backtrack.Core.Application.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity, TKey> where TEntity : class
    {
        Task<TEntity> CreateAsync(TEntity entity);
        Task<TEntity?> GetByIdAsync(TKey id, bool isTrack = false);
        void Update(TEntity entity);
        Task<bool> DeleteAsync(TKey id);
        Task SaveChangesAsync();
    }
}
