using System.Text.Json;
using System.Text.Json.Serialization;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class OrgFormTemplateConfiguration : IEntityTypeConfiguration<OrgFormTemplate>
{
    public void Configure(EntityTypeBuilder<OrgFormTemplate> builder)
    {
        builder.ToTable("org_form_templates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(t => t.OrgId)
            .HasColumnName("org_id")
            .IsRequired();

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        var fieldsConverter = new ValueConverter<List<FormFieldDefinition>, string>(
            toDb => JsonSerializer.Serialize(toDb, jsonOptions),
            fromDb => JsonSerializer.Deserialize<List<FormFieldDefinition>>(fromDb, jsonOptions) ?? new List<FormFieldDefinition>()
        );

        var fieldsComparer = new ValueComparer<List<FormFieldDefinition>>(
            (a, b) => JsonSerializer.Serialize(a, jsonOptions) == JsonSerializer.Serialize(b, jsonOptions),
            v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOptions).GetHashCode(),
            v => v == null ? new List<FormFieldDefinition>() : JsonSerializer.Deserialize<List<FormFieldDefinition>>(JsonSerializer.Serialize(v, jsonOptions), jsonOptions)!
        );

        builder.Property(t => t.Fields)
            .HasColumnName("fields")
            .HasColumnType("jsonb")
            .HasConversion(fieldsConverter, fieldsComparer)
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(t => t.DeletedAt)
            .HasColumnName("deleted_at");

        // Relationships
        builder.HasOne(t => t.Organization)
            .WithMany()
            .HasForeignKey(t => t.OrgId)
            .HasConstraintName("fk_org_form_templates_org_id")
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes - one org can have only one template
        builder.HasIndex(t => t.OrgId)
            .IsUnique()
            .HasDatabaseName("ix_org_form_templates_org_id")
            .HasFilter("deleted_at IS NULL");

        // Global query filter for soft delete
        builder.HasQueryFilter(t => t.DeletedAt == null);
    }
}
