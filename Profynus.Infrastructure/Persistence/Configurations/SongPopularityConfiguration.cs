using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Audio.Entities;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class SongPopularityConfiguration : IEntityTypeConfiguration<SongPopularity>
{
    public void Configure(EntityTypeBuilder<SongPopularity> b)
    {
        b.ToTable("song_popularity");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x=>x.SongId).HasColumnName("song_id");
        b.Property(x=>x.Period).HasColumnName("period");
        b.Property(x=>x.PeriodStart).HasColumnName("period_start");
        b.Property(x=>x.PlayCount).HasColumnName("play_count");
        b.Property(x=>x.UniqueListeners).HasColumnName("unique_listeners");
        b.Property(x=>x.CompletePlayCount).HasColumnName("complete_play_count");
        b.Property(x=>x.SkipCount).HasColumnName("skip_count");
        b.Property(x=> x.AverageListenPlayCount).HasColumnName("avg_listen_pct");
        b.Property(x=>x.TotalListenSecs).HasColumnName("total_listen_secs");
        b.Property(x=>x.PopularityScore).HasColumnName("popularity_score");
        b.Property(x=>x.ComputedAt).HasColumnName("computed_at");
    }
}