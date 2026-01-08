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

        public virtual async Task<TEntity?> GetByIdAsync(TKey id)
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    Equals(e.Id, id) &&
                    e.DeletedAt == null);
        }

        public virtual bool Update(TEntity entity)
        {
            entity.UpdatedAt = DateTimeOffset.UtcNow;
            _context.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            _context.Entry(entity).Property(x => x.CreatedAt).IsModified = false;
            _context.Entry(entity).Property(x => x.DeletedAt).IsModified = false;

            return true;
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