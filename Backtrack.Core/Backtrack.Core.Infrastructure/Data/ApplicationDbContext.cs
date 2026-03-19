
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<JoinInvitation> JoinInvitations { get; set; }
        public DbSet<PostMatch> PostMatches { get; set; }
        public DbSet<OrganizationInventory> OrganizationInventories { get; set; }
        public DbSet<PostImage> PostImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new MembershipConfiguration());
            modelBuilder.ApplyConfiguration(new JoinInvitationConfiguration());
            modelBuilder.ApplyConfiguration(new PostMatchConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationInventoryConfiguration());
            modelBuilder.ApplyConfiguration(new PostImageConfiguration());
        }
    }
}