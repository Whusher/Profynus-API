using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Audio.Entities;
using Profynus.Domain.Audio.Enums;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class ListeningEventConfiguration : IEntityTypeConfiguration<ListeningEvent>
{
    public void Configure(EntityTypeBuilder<ListeningEvent> b)
    {
        b.ToTable("listening_events");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x=>x.SongId).HasColumnName("song_id");
        b.Property(x=>x.UserId).HasColumnName("user_id");
        b.Property(x=>x.ShareUrlId).HasColumnName("share_url_id");
        b.Property(x=>x.EventType).HasColumnName("event_id")
            .HasConversion(v => v.ToString(),
                v => Enum.Parse<ListenEventType>(v));
        b.Property(x=>x.PositionSecs).HasColumnName("position_secs");
        b.Property(x=>x.SessionId).HasColumnName("session_id");
        b.Property(x=>x.ClientIp).HasColumnName("client_ip");
        b.Property(x=>x.UserAgent).HasColumnName("user_agent");
        b.Property(x => x.CountryCode).HasColumnName("country_code");
        b.Property(x=>x.Metadata).HasColumnName("metadata");
        b.Property(x=>x.OccurredAt).HasColumnName("occurred_at");
    }
}