using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Audio.Entities;
using Profynus.Domain.Audio.Enums;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class SongShareUrlConfiguration : IEntityTypeConfiguration<SongShareUrl>
{
    public void Configure(EntityTypeBuilder<SongShareUrl> b)
    {
        b.ToTable("song_share_urls");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.SongId).HasColumnName("song_id");
        b.Property(x=>x.CreatedBy).HasColumnName("created_by");
        b.Property(x=>x.Token).HasColumnName("token");
        b.Property(x=>x.Status).HasColumnName("status")
            .HasConversion(v =>v.ToString(), v => Enum.Parse<UrlStatus>(v));
        b.Property(x=> x.ExpiresAt).HasColumnName("expires_at");
        b.Property(x=> x.MaxAccesses).HasColumnName("max_accesses");
        b.Property(x=> x.AccessCount).HasColumnName("access_count");
        b.Property(x=> x.IPWhitelist).HasColumnName("ip_whitelist");
        b.Property(x=> x.Metadata).HasColumnName("metadata");
        b.Property(x=> x.CreatedAt).HasColumnName("created_at");
    }
}