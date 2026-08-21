using Microsoft.EntityFrameworkCore;
using Profynus.Domain.Auth.Entities;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Auth.Enums;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class DeviceConfiguration : IEntityTypeConfiguration<Device>
{
    private static readonly JsonSerializerOptions JsonOpts = new();

    public void Configure(EntityTypeBuilder<Device> e)
    {
        e.ToTable("devices");
        e.HasKey(d => d.Id);

        e.Property(d => d.Id).HasColumnName("id");
        e.Property(d => d.UserId).HasColumnName("user_id");
        e.Property(d => d.ClientType)
            .HasColumnName("client_type")
            .HasConversion(
                v => v.ToString(),           // C# → DB:  Argon2id → "Argon2id"
                v => Enum.Parse<ClientType>(v));  // DB → C#;
        e.Property(d => d.Platform).HasColumnName("platform");
        e.Property(d => d.Os).HasColumnName("os");
        e.Property(d => d.AppVersion).HasColumnName("app_version");
        e.Property(d => d.UaHash).HasColumnName("ua_hash");
        // Tell EF this is a JSONB column, not a navigation to another table
        e.Property(d => d.Fingerprint)
            .HasColumnName("fingerprint")
            .HasColumnType("jsonb")
            .HasConversion(
                v => v == null ? null : JsonSerializer.Serialize(v, JsonOpts),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(v, JsonOpts))
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, object>?>(
                (a, b) => JsonSerializer.Serialize(a, JsonOpts) ==
                          JsonSerializer.Serialize(b, JsonOpts),
                v => v == null ? 0 : JsonSerializer.Serialize(v, JsonOpts).GetHashCode(),
                v => v == null ? null : JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(v, JsonOpts), JsonOpts)));
        e.Property(d => d.MacHash).HasColumnName("mac_hash");
        e.Property(d => d.Name).HasColumnName("name");
        e.Property(d => d.IsTrusted).HasColumnName("is_trusted");
        e.Property(d => d.IsActive).HasColumnName("is_active");
        e.Property(d => d.FirstSeenAt).HasColumnName("first_seen_at");
        e.Property(d => d.LastSeenAt).HasColumnName("last_seen_at");
        e.Property(d => d.RevokedAt).HasColumnName("revoked_at");
    
        // Indexing for query
        e.HasIndex(d => d.UserId );
        e.HasIndex(d => new { d.UserId, d.IsActive });
    }
}