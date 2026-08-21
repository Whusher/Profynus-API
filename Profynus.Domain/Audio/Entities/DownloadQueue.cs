using Profynus.Domain.Audio.Enums;

namespace Profynus.Domain.Audio.Entities;

public class DownloadQueue
{
    public Guid Id { get; set; }
    public Guid SongId { get; set; }
    public Guid UserId { get; set; }
    public Guid ShareUrlId { get; set; }

    // Queue control
    public DownloadStatus Status { get; set; }
    public DownloadPriority Priority { get; set; }
    
    // Scheduling & retries
    public DateTimeOffset ScheduledAt { get; set; }
    public int RetryCount { get; set; }
    public int MaxRetries { get; set; }
    public DateTimeOffset NextRetryAt { get; set; }
    public string ErrorMessage { get; set; }
    
    // Processing bookkeeping
    public DateTimeOffset ProcessingStartedAt { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
    
    // Throttling: backend groups by this key to enforce per-user or per-provider limits
    public string RateLimitKey { get; set; }
    public int EstimatedSizeBytes { get; set; }
    
    // Metadata control
    public string Metadata { get; set; }
    
    // Date controls
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}