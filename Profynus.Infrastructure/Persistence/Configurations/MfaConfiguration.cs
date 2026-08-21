using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Auth.Entities;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class MfaConfiguration : IEntityTypeConfiguration<MfaConfig>
{
    public void Configure(EntityTypeBuilder<MfaConfig> e)
    {
        e.ToTable("mfa_configs");
        
        e.HasKey(m => m.Id);
        
        e.Property(m => m.Id).HasColumnName("id");
        e.Property(m => m.UserId).HasColumnName("user_id");
        e.Property(m  => m.Method).HasColumnName("method").HasConversion<string>().HasMaxLength(50);
        e.Property(m => m.SecretEnc).HasColumnName("secret_enc");
        e.Property(m => m.IsVerified).HasColumnName("is_primary");
        e.Property(m => m.IsVerified).HasColumnName("is_verified");
        e.Property(m => m.CreatedAt).HasColumnName("created_at");
        e.Property(m => m.LastUsedAt).HasColumnName("last_used_at");
        
        e.HasIndex(m => m.UserId);
    }
}