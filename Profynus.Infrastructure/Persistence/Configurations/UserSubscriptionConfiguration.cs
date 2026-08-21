using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Subscription.Entities;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class UserSubscriptionConfiguration : IEntityTypeConfiguration<UserSubscription>
{
    public void Configure(EntityTypeBuilder<UserSubscription> b)
    {
        b.ToTable("user_subscriptions");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x => x.ExternalId).HasColumnName("external_id");
        b.Property(x=>x.Email).HasColumnName("email");
        b.Property(x=>x.Plan).HasColumnName("plan");
        b.Property(x=> x.DownloadQuota).HasColumnName("download_quota");
        b.Property(x=>x.UploadQuota).HasColumnName("upload_quota");
        b.Property(x=>x.CreatedAt).HasColumnName("created_at");
        b.Property(x=>x.UpdatedAt).HasColumnName("updated_at");
    }
}