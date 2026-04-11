using System.Text.Json;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("id").IsRequired();
        builder.Property(s => s.SubscriberType).HasColumnName("subscriber_type").HasConversion<string>().IsRequired();
        builder.Property(s => s.UserId).HasColumnName("user_id").HasMaxLength(128);
        builder.Property(s => s.OrganizationId).HasColumnName("organization_id");
        builder.Property(s => s.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(s => s.ProviderSubscriptionId).HasColumnName("provider_subscription_id").HasMaxLength(255).IsRequired();
        builder.Property(s => s.ProviderCustomerId).HasColumnName("provider_customer_id").HasMaxLength(255).IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(s => s.CurrentPeriodStart).HasColumnName("current_period_start").IsRequired();
        builder.Property(s => s.CurrentPeriodEnd).HasColumnName("current_period_end").IsRequired();
        builder.Property(s => s.CancelAtPeriodEnd).HasColumnName("cancel_at_period_end").IsRequired().HasDefaultValue(false);

        var jsonOpts = new JsonSerializerOptions();

        builder.Property(s => s.PlanSnapshot)
            .HasColumnName("plan_snapshot")
            .HasColumnType("jsonb")
            .IsRequired()
            .HasConversion(
                new ValueConverter<SubscriptionPlanSnapshot, string>(
                    v => JsonSerializer.Serialize(v, jsonOpts),
                    v => JsonSerializer.Deserialize<SubscriptionPlanSnapshot>(v, jsonOpts)!),
                new ValueComparer<SubscriptionPlanSnapshot>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOpts) == JsonSerializer.Serialize(b, jsonOpts),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOpts).GetHashCode(),
                    v => JsonSerializer.Deserialize<SubscriptionPlanSnapshot>(JsonSerializer.Serialize(v, jsonOpts), jsonOpts)!));

        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Property(s => s.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(s => s.Plan)
            .WithMany()
            .HasForeignKey(s => s.PlanId)
            .HasConstraintName("fk_subscriptions_plan_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Organization)
            .WithMany()
            .HasForeignKey(s => s.OrganizationId)
            .HasConstraintName("fk_subscriptions_organization_id")
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        builder.HasIndex(s => s.ProviderSubscriptionId)
            .IsUnique()
            .HasDatabaseName("ux_subscriptions_provider_subscription_id");

        builder.HasIndex(s => new { s.UserId, s.Status })
            .HasDatabaseName("ix_subscriptions_user_id_status");

        builder.HasIndex(s => new { s.OrganizationId, s.Status })
            .HasDatabaseName("ix_subscriptions_organization_id_status");

        builder.HasQueryFilter(s => s.DeletedAt == null);
    }
}
