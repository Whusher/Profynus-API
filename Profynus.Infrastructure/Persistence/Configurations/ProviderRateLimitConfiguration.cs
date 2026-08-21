using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Provider.Entities;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class ProviderRateLimitConfiguration : IEntityTypeConfiguration<ProviderRateLimits>
{
    public void Configure(EntityTypeBuilder<ProviderRateLimits> b)
    {
        b.ToTable("provider_rate_limits");
        b.HasKey(x => x.Id);
        
        b.Property(x => x.Id).HasColumnName("id");
        b.Property(x=>x.Provider).HasColumnName("provider");
        b.Property(x=>x.WindowType).HasColumnName("window_type");
        b.Property(x=>x.WindowStart).HasColumnName("window_start");
        b.Property(x=>x.RequestCount).HasColumnName("request_count");
        b.Property(x=>x.LimitCapacity).HasColumnName("limit_cap");
        b.Property(x=>x.UpdatedAt).HasColumnName("updated_at");
    }
}