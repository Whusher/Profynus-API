namespace Profynus.Domain.Subscription.Entities;

// =============================================================================
//  USER DOWNLOAD RATE LIMITS
// 
// Strategy:
// • Sliding-window counters: backend increments on each successful download.
// • window_start marks the beginning of the current window.
// • Backend checks: count < limit before dequeuing a new job.
// • Row is upserted per (user_id, window_type) each time a download fires.
//=============================================================================
public class UserDownloadLimits
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string WindowType { get; set; } //'hourly', 'daily', 'monthly'
    public DateTimeOffset WindowStart { get; set; }
    public int DownloadCount { get; set; }
    public int LimitCapacity { get; set; } // Configurable per plan
    public DateTimeOffset UpdatedAt { get; set; }
}