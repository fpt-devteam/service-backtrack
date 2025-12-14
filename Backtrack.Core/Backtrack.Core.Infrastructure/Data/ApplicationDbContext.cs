
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }
}