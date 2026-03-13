using Backtrack.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pgvector;
using System.Collections;

namespace Backtrack.Core.Infrastructure.Data.Configurations
{
    public class OrganizationInventoryConfiguration : IEntityTypeConfiguration<OrganizationInventory>
    {
        public void Configure(EntityTypeBuilder<OrganizationInventory> builder)
        {
            builder.ToTable("org_inventories");

            builder.HasKey(oi => oi.Id);

            builder.Property(oi => oi.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(oi => oi.OrgId)
                .HasColumnName("org_id")
                .IsRequired();

            builder.HasOne(oi => oi.Organization)
                .WithMany()
                .HasForeignKey(oi => oi.OrgId)
                .HasConstraintName("fk_org_inventories_org_id_organizations_id")
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(oi => oi.LoggedById)
                .HasColumnName("logged_by_id")
                .HasColumnType("text")
                .IsRequired();

            builder.HasOne(oi => oi.LoggedBy)
                .WithMany()
                .HasForeignKey(oi => oi.LoggedById)
                .HasConstraintName("fk_org_inventories_logged_by_id_users_id")
                .OnDelete(DeleteBehavior.NoAction);

            builder.Property(oi => oi.ItemName)
                .HasColumnName("item_name")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(oi => oi.Description)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired();

            builder.Property(oi => oi.DistinctiveMarks)
                .HasColumnName("distinctive_marks")
                .HasMaxLength(500);

            builder.Property(oi => oi.ImageUrls)
                .HasColumnName("image_urls")
                .HasColumnType("text[]")
                .IsRequired();

            builder.Property(oi => oi.StorageLocation)
                .HasColumnName("storage_location")
                .HasMaxLength(500);

            // Vector converter for embeddings
            var embeddingToVectorConverter = new ValueConverter<float[]?, Vector?>(
                toDb => toDb == null ? null : new Vector(toDb),
                fromDb => fromDb == null ? null : fromDb.ToArray()
            );

            var embeddingComparer = new ValueComparer<float[]?>(
                (a, b) =>
                    a == b || (a != null && b != null && a.SequenceEqual(b)),
                v =>
                    v == null ? 0 : StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v =>
                    v == null ? null : v.ToArray()
            );

            builder.Property(oi => oi.ContentEmbedding)
                .HasColumnName("content_embedding")
                .HasColumnType("vector(768)")
                .HasConversion(embeddingToVectorConverter, embeddingComparer);

            builder.Property(oi => oi.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(oi => oi.LoggedAt)
                .HasColumnName("logged_at")
                .IsRequired();

            builder.Property(oi => oi.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(oi => oi.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(oi => oi.DeletedAt)
                .HasColumnName("deleted_at");

            // Indexes
            builder.HasIndex(oi => oi.OrgId)
                .HasDatabaseName("ix_org_inventories_org_id");

            builder.HasIndex(oi => oi.LoggedById)
                .HasDatabaseName("ix_org_inventories_logged_by_id");

            // Vector index for content embedding
            builder.HasIndex(oi => oi.ContentEmbedding)
                .HasDatabaseName("ix_org_inventories_content_embedding")
                .HasMethod("hnsw")
                .HasOperators("vector_cosine_ops");

            // Global query filter for soft delete
            builder.HasQueryFilter(oi => oi.DeletedAt == null);
        }
    }
}
