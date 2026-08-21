using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Audio.Entities;
using Profynus.Domain.Audio.Enums;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class DownloadQueueConfiguration : IEntityTypeConfiguration<DownloadQueue>
{
    public void Configure(EntityTypeBuilder<DownloadQueue> builder)
    {
        builder.ToTable("download_queue");
        
        // Primary Key
        builder.HasKey(x => x.Id);
        
        // Table columns specification
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.SongId).HasColumnName("song_id");
        builder.Property(x=> x.UserId).HasColumnName("user_id");
        builder.Property(x=>x.ShareUrlId).HasColumnName("share_url_id");
        builder.Property(x=>x.Status).HasColumnName("status")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<DownloadStatus>(v));
        builder.Property(x => x.Priority).HasColumnName("priority")
            .HasConversion(
                v => v.ToString(),
                v => Enum.Parse<DownloadPriority>(v));
        builder.Property(x => x.ScheduledAt).HasColumnName("scheduled_at");
        builder.Property(x => x.RetryCount).HasColumnName("retry_count");
        builder.Property(x=> x.MaxRetries).HasColumnName("max_retries");
        builder.Property(x => x.NextRetryAt).HasColumnName("next_retry_at");
        builder.Property(x => x.ErrorMessage).HasColumnName("error_message");
        builder.Property(x=>x.ProcessingStartedAt).HasColumnName("processing_started_at");
        builder.Property(x=>x.CompletedAt).HasColumnName("completed_at");
        builder.Property(x => x.RateLimitKey).HasColumnName("rate_limit_key");
        builder.Property(x=> x.EstimatedSizeBytes).HasColumnName("estimated_size_bytes");
        builder.Property(x=>x.Metadata).HasColumnName("metadata");
        builder.Property(x=> x.CreatedAt).HasColumnName("created_at");
        builder.Property(x=> x.UpdatedAt).HasColumnName("updated_at");
    }
}