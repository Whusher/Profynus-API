using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Auth.Entities;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> e)
    {
        e.ToTable("sessions");
        e.HasKey(x => x.Id);

        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.UserId).HasColumnName("user_id");
        e.Property(x => x.DeviceId).HasColumnName("device_id");
        e.Property(x => x.AccessTokenHash).HasColumnName("access_token_hash").HasMaxLength(64);
        e.Property(x => x.RefreshTokenHash).HasColumnName("refresh_token_hash").HasMaxLength(64);
        e.Property(x => x.AccessExpiresAt).HasColumnName("access_expires_at");
        e.Property(x => x.RefreshExpiresAt).HasColumnName("refresh_expires_at");
        e.Property(x => x.IpAddress)
            .HasColumnName("ip_address");
        e.Property(x => x.GeoCountry).HasColumnName("geo_country").HasMaxLength(2);
        e.Property(x => x.GeoCity).HasColumnName("geo_city").HasMaxLength(100);
        e.Property(x => x.IsActive).HasColumnName("is_active");
        e.Property(x => x.CreatedAt).HasColumnName("created_at");
        e.Property(x => x.LastUsedAt).HasColumnName("last_used_at");
        e.Property(x => x.RevokedAt).HasColumnName("revoked_at");
        e.Property(x => x.RevokeReason).HasColumnName("revoke_reason").HasMaxLength(100);

        // Unique constraints from the SQL schema
        e.HasIndex(x => x.AccessTokenHash).IsUnique();
        e.HasIndex(x => x.RefreshTokenHash).IsUnique();

        // Relationships (required FKs in DB, but optional navigation in EF model
        // to play nicely with User's soft-delete global query filter)
        e.HasOne(s => s.User)
            .WithMany(u => u.Sessions)
            .HasForeignKey(s => s.UserId)
            .IsRequired(false);

        e.HasOne(s => s.Device)
            .WithMany(d => d.Sessions)
            .HasForeignKey(s => s.DeviceId)
            .IsRequired(false);

        // Useful query indexes
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => new { x.UserId, x.IsActive });
    }
}