using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class PaymentHistoryConfiguration : IEntityTypeConfiguration<PaymentHistory>
{
    public void Configure(EntityTypeBuilder<PaymentHistory> builder)
    {
        builder.ToTable("payment_histories");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasColumnName("id").IsRequired();
        builder.Property(p => p.SubscriptionId).HasColumnName("subscription_id").IsRequired();
        builder.Property(p => p.SubscriberType).HasColumnName("subscriber_type").HasConversion<string>().IsRequired();
        builder.Property(p => p.UserId).HasColumnName("user_id").HasMaxLength(128);
        builder.Property(p => p.OrganizationId).HasColumnName("organization_id");
        builder.Property(p => p.ProviderInvoiceId).HasColumnName("provider_invoice_id").HasMaxLength(255).IsRequired();
        builder.Property(p => p.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(3).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>().IsRequired();
        builder.Property(p => p.PaymentDate).HasColumnName("payment_date").IsRequired();
        builder.Property(p => p.InvoiceUrl).HasColumnName("invoice_url").HasMaxLength(2048);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        builder.HasOne(p => p.Subscription)
            .WithMany()
            .HasForeignKey(p => p.SubscriptionId)
            .HasConstraintName("fk_payment_histories_subscription_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.SubscriptionId).HasDatabaseName("ix_payment_histories_subscription_id");
        builder.HasIndex(p => p.ProviderInvoiceId).IsUnique().HasDatabaseName("ux_payment_histories_provider_invoice_id");

        builder.HasQueryFilter(p => p.DeletedAt == null);
    }
}
