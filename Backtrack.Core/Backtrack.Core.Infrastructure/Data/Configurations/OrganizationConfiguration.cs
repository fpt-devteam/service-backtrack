using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backtrack.Core.Domain.Entities;
using Backtrack.Core.Domain.ValueObjects;

namespace Backtrack.Core.Infrastructure.Data.Configurations
{
    public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
    {
        public void Configure(EntityTypeBuilder<Organization> builder)
        {
            builder.ToTable("organizations");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Id)
                .HasColumnName("id")
                .IsRequired();

            builder.Property(o => o.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(o => o.Slug)
                .HasColumnName("slug")
                .HasMaxLength(255)
                .IsRequired();

            builder.HasIndex(o => o.Slug)
                .IsUnique()
                .HasDatabaseName("ix_organizations_slug");

            var geoPointToPointConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<GeoPoint, NetTopologySuite.Geometries.Point>(
                toDb => new NetTopologySuite.Geometries.Point(toDb.Longitude, toDb.Latitude) { SRID = 4326 },
                fromDb => new GeoPoint(fromDb.Y, fromDb.X)
            );

            var geoPointComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<GeoPoint>(
                (a, b) => a != null && b != null && a.Latitude == b.Latitude && a.Longitude == b.Longitude,
                v => v == null ? 0 : HashCode.Combine(v.Latitude, v.Longitude),
                v => new GeoPoint(v.Latitude, v.Longitude)
            );

            builder.Property(o => o.Location)
                .HasColumnName("location")
                .HasColumnType("geography(point, 4326)")
                .HasConversion(geoPointToPointConverter, geoPointComparer)
                .IsRequired();

            builder.Property(o => o.DisplayAddress)
                .HasColumnName("display_address")
                .HasMaxLength(1000)
                .IsRequired();

            builder.Property(o => o.ExternalPlaceId)
                .HasColumnName("external_place_id")
                .HasMaxLength(500);

            builder.Property(o => o.Phone)
                .HasColumnName("phone")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(o => o.IndustryType)
                .HasColumnName("industry_type")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(o => o.TaxIdentificationNumber)
                .HasColumnName("tax_identification_number")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(o => o.LogoUrl)
                .HasColumnName("logo_url")
                .HasMaxLength(2048)
                .IsRequired();

            builder.Property(o => o.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            builder.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at");

            builder.Property(o => o.DeletedAt)
                .HasColumnName("deleted_at");

            builder.HasQueryFilter(o => o.DeletedAt == null);
        }
    }
}
