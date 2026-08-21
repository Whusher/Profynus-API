namespace Profynus.Infrastructure.Persistence.Configurations;

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Profynus.Domain.Auth.Entities;



public class AuthEventConfiguration : IEntityTypeConfiguration<AuthEvent>
{
    private static readonly JsonSerializerOptions JsonOpts = new();

    public void Configure(EntityTypeBuilder<AuthEvent> e)
    {
        e.ToTable("auth_events");

        e.HasKey(x => x.Id);

        e.Property(x => x.Id).HasColumnName("id");
        e.Property(x => x.UserId).HasColumnName("user_id");
        e.Property(x => x.SessionId).HasColumnName("session_id");
        e.Property(x => x.DeviceId).HasColumnName("device_id");
        e.Property(x => x.EventType).HasColumnName("event_type")
            .HasConversion<string>().HasMaxLength(50);
        e.Property(x => x.Status).HasColumnName("status")
            .HasConversion<string>().HasMaxLength(20);
        e.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(45);
        e.Property(x => x.FailureReason).HasColumnName("failure_reason");
        e.Property(x => x.OccurredAt).HasColumnName("occurred_at");

        // Tell EF this is a JSONB column, not a navigation to another table
        e.Property(x => x.Metadata)
            .HasColumnName("metadata")
            .HasColumnType("jsonb")
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOpts),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, JsonOpts)
                     ?? new Dictionary<string, object>())
            .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, object>>(
                (a, b) => JsonSerializer.Serialize(a, JsonOpts) ==
                          JsonSerializer.Serialize(b, JsonOpts),
                v => JsonSerializer.Serialize(v, JsonOpts).GetHashCode(),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(
                    JsonSerializer.Serialize(v, JsonOpts), JsonOpts)!));

        e.HasIndex(x => new { x.UserId, x.OccurredAt });
        e.HasIndex(x => x.OccurredAt);
    }
}