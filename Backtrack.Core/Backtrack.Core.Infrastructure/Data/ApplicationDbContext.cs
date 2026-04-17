
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Backtrack.Core.Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Subcategory> Subcategories { get; set; }
        public DbSet<PostPersonalBelongingDetail> PostPersonalBelongingDetails { get; set; }
        public DbSet<PostCardDetail> PostCardDetails { get; set; }
        public DbSet<PostElectronicDetail> PostElectronicDetails { get; set; }
        public DbSet<PostOtherDetail> PostOtherDetails { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<JoinInvitation> JoinInvitations { get; set; }
        public DbSet<PostMatch> PostMatches { get; set; }
        public DbSet<C2CReturnReport> C2CReturnReports { get; set; }
        public DbSet<OrgReturnReport> OrgReturnReports { get; set; }
        public DbSet<OrgReceiveReport> OrgReceiveReports { get; set; }
        public DbSet<QrCode> QrCodes { get; set; }
        public DbSet<QrDesign> QrDesigns { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<PaymentHistory> PaymentHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PostConfiguration());
            modelBuilder.ApplyConfiguration(new SubcategoryConfiguration());
            modelBuilder.ApplyConfiguration(new PostPersonalBelongingDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PostCardDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PostElectronicDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PostOtherDetailConfiguration());
            modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
            modelBuilder.ApplyConfiguration(new MembershipConfiguration());
            modelBuilder.ApplyConfiguration(new JoinInvitationConfiguration());
            modelBuilder.ApplyConfiguration(new PostMatchConfiguration());
            modelBuilder.ApplyConfiguration(new C2CReturnReportConfiguration());
            modelBuilder.ApplyConfiguration(new OrgReturnReportConfiguration());
            modelBuilder.ApplyConfiguration(new OrgReceiveReportConfiguration());
            modelBuilder.ApplyConfiguration(new QrCodeConfiguration());
            modelBuilder.ApplyConfiguration(new QrDesignConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionPlanConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentHistoryConfiguration());
        }
    }
}
