using Backtrack.Core.Application.Common.Interfaces.Repositories;
using Backtrack.Core.Domain.Common;
using Backtrack.Core.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Repositories.Common
{
    public class CrudRepositoryBase<TEntity, TKey>
    : IGenericRepository<TEntity, TKey>
    where TEntity : Entity<TKey>
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<TEntity> _dbSet;

        protected CrudRepositoryBase(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public virtual async Task<TEntity> CreateAsync(TEntity entity)
        {
            entity.CreatedAt = DateTimeOffset.UtcNow;
            await _dbSet.AddAsync(entity);
            return entity;
        }

        public virtual async Task<TEntity?> GetByIdAsync(TKey id, bool isTrack = false)
        {
            IQueryable<TEntity> query = _dbSet;
            if (!isTrack)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(e =>
                Equals(e.Id, id) &&
                e.DeletedAt == null);
        }

        public virtual void Update(TEntity entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached) throw new InvalidOperationException("Entity to update is not being tracked. Ensure the entity is retrieved from the same context instance before updating.");
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            entry.Property(x => x.CreatedAt).IsModified = false;
            entry.Property(x => x.DeletedAt).IsModified = false;
        }

        public virtual async Task<bool> DeleteAsync(TKey id)
        {
            var entity = await _dbSet.FirstOrDefaultAsync(e =>
                Equals(e.Id, id) &&
                e.DeletedAt == null);

            if (entity == null) return false;

            entity.DeletedAt = DateTimeOffset.UtcNow;
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            return true;
        }

        public virtual async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}