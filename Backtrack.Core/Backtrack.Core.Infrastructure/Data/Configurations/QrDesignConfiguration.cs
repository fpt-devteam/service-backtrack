using System.Text.Json;
using Backtrack.Core.Domain.Constants;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Backtrack.Core.Infrastructure.Data.Configurations;

public class QrDesignConfiguration : IEntityTypeConfiguration<QrDesign>
{
    public void Configure(EntityTypeBuilder<QrDesign> builder)
    {
        builder.ToTable("qr_designs");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id").IsRequired();
        builder.Property(d => d.UserId).HasColumnName("user_id").HasMaxLength(128).IsRequired();
        builder.Property(d => d.ForegroundColor).HasColumnName("foreground_color").HasMaxLength(20).IsRequired();
        builder.Property(d => d.BackgroundColor).HasColumnName("background_color").HasMaxLength(20).IsRequired();
        builder.Property(d => d.DotStyle).HasColumnName("dot_style").HasMaxLength(30).IsRequired();
        builder.Property(d => d.CornerSquareStyle).HasColumnName("corner_square_style").HasMaxLength(30).IsRequired();
        builder.Property(d => d.CornerSquareColor).HasColumnName("corner_square_color").HasMaxLength(20).IsRequired();
        builder.Property(d => d.CornerDotStyle).HasColumnName("corner_dot_style").HasMaxLength(30).IsRequired();
        builder.Property(d => d.CornerDotColor).HasColumnName("corner_dot_color").HasMaxLength(20).IsRequired();
        builder.Property(d => d.ErrorCorrectionLevel).HasColumnName("error_correction_level").HasConversion<string>().IsRequired();

        var jsonOpts = new JsonSerializerOptions();

        builder.Property(d => d.Logo)
            .HasColumnName("logo")
            .HasColumnType("jsonb")
            .HasConversion(
                new ValueConverter<QrLogoSettings?, string?>(
                    v => v == null ? null : JsonSerializer.Serialize(v, jsonOpts),
                    v => v == null ? null : JsonSerializer.Deserialize<QrLogoSettings>(v, jsonOpts)),
                new ValueComparer<QrLogoSettings?>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOpts) == JsonSerializer.Serialize(b, jsonOpts),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOpts).GetHashCode(),
                    v => v == null ? null : JsonSerializer.Deserialize<QrLogoSettings>(JsonSerializer.Serialize(v, jsonOpts), jsonOpts)));

        builder.Property(d => d.Gradient)
            .HasColumnName("gradient")
            .HasColumnType("jsonb")
            .HasConversion(
                new ValueConverter<QrGradientSettings?, string?>(
                    v => v == null ? null : JsonSerializer.Serialize(v, jsonOpts),
                    v => v == null ? null : JsonSerializer.Deserialize<QrGradientSettings>(v, jsonOpts)),
                new ValueComparer<QrGradientSettings?>(
                    (a, b) => JsonSerializer.Serialize(a, jsonOpts) == JsonSerializer.Serialize(b, jsonOpts),
                    v => v == null ? 0 : JsonSerializer.Serialize(v, jsonOpts).GetHashCode(),
                    v => v == null ? null : JsonSerializer.Deserialize<QrGradientSettings>(JsonSerializer.Serialize(v, jsonOpts), jsonOpts)));

        builder.Property(d => d.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(d => d.UpdatedAt).HasColumnName("updated_at");
        builder.Property(d => d.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(d => d.UserId).IsUnique().HasDatabaseName("ux_qr_designs_user_id").HasFilter("deleted_at IS NULL");

        builder.HasQueryFilter(d => d.DeletedAt == null);
    }
}
