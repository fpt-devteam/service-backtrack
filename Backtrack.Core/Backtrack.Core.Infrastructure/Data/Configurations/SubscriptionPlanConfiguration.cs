using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("subscription_plans");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(255).IsRequired();
        builder.Property(p => p.Price).HasColumnName("price").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(p => p.BillingInterval).HasColumnName("billing_interval").HasConversion<string>().IsRequired();
        builder.Property(p => p.SubscriberType).HasColumnName("subscriber_type").HasConversion<string>().IsRequired();
        builder.Property(p => p.ProviderPriceId).HasColumnName("provider_price_id").HasMaxLength(255).IsRequired();
        builder.Property(p => p.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

        builder.Property(p => p.Features)
            .HasColumnName("features")
            .HasColumnType("text[]")
            .IsRequired()
            .Metadata.SetValueComparer(new ValueComparer<string[]>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v == null ? 0 : v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
                v => v == null ? Array.Empty<string>() : v.ToArray()));

        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
