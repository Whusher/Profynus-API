namespace Profynus.Domain.Subscription.Entities;

public class UserSubscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalId { get; set; }          // ID from auth provider (Clerk, Auth0, etc.)
    public string Email { get; set; }
    public string Plan { get; set; }                // 'free', 'premium', etc.
    public int DownloadQuota { get; set; }          //  max concurrent/daily downloads allowed
    public int UploadQuota { get; set; }            //  max upload songs allowed
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}