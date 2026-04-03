using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class P2PHandoverConfiguration : IEntityTypeConfiguration<P2PHandover>
{
    public void Configure(EntityTypeBuilder<P2PHandover> builder)
    {
        builder.ToTable("p2p_handovers");

        builder.Property(h => h.FinderId)
            .HasColumnName("finder_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(h => h.OwnerId)
            .HasColumnName("owner_id")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(h => h.FinderPostId)
            .HasColumnName("finder_post_id");

        builder.Property(h => h.OwnerPostId)
            .HasColumnName("owner_post_id");

        builder.HasOne(h => h.Finder)
            .WithMany()
            .HasForeignKey(h => h.FinderId)
            .HasConstraintName("fk_p2p_handovers_finder_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Owner)
            .WithMany()
            .HasForeignKey(h => h.OwnerId)
            .HasConstraintName("fk_p2p_handovers_owner_id")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.FinderPost)
            .WithMany()
            .HasForeignKey(h => h.FinderPostId)
            .HasConstraintName("fk_p2p_handovers_finder_post_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(h => h.OwnerPost)
            .WithMany()
            .HasForeignKey(h => h.OwnerPostId)
            .HasConstraintName("fk_p2p_handovers_owner_post_id")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(h => h.FinderId)
            .HasDatabaseName("ix_p2p_handovers_finder_id");

        builder.HasIndex(h => h.OwnerId)
            .HasDatabaseName("ix_p2p_handovers_owner_id");

        builder.HasIndex(h => h.FinderPostId)
            .HasDatabaseName("ix_p2p_handovers_finder_post_id");

        builder.HasIndex(h => h.OwnerPostId)
            .HasDatabaseName("ix_p2p_handovers_owner_post_id");
    }
}
