using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Subscription.Entities;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class UserDownloadLimitConfiguration : IEntityTypeConfiguration<UserDownloadLimits>
{
    public void Configure(EntityTypeBuilder<UserDownloadLimits> b)
    {
        b.ToTable("user_download_limits");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x=>x.UserId).HasColumnName("user_id");
        b.Property(x=>x.WindowType).HasColumnName("window_type");
        b.Property(x=>x.WindowStart).HasColumnName("window_start");
        b.Property(x=>x.DownloadCount).HasColumnName("download_count");
        b.Property(x=>x.LimitCapacity).HasColumnName("limit_cap");
        b.Property(x=>x.UpdatedAt).HasColumnName("updated_at");
    }
}