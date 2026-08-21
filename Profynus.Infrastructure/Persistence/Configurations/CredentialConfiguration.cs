using Microsoft.EntityFrameworkCore;
using Profynus.Domain.Auth.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.Auth.Enums;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class CredentialConfiguration : IEntityTypeConfiguration<Credential>
{
    public void Configure(EntityTypeBuilder<Credential> e)
    {
        e.ToTable("credentials");
        e.HasKey(x => x.Id);
        
        e.Property(c => c.Id).HasColumnName("id");
        e.Property(c => c.UserId).HasColumnName("user_id");
        e.Property(c => c.PasswordHash).HasColumnName("password_hash");
        e.Property(x => x.Algorithm)
            .HasColumnName("algorithm")
            .HasMaxLength(20)
            .HasConversion(
                v => v.ToString(),           // C# → DB:  Argon2id → "Argon2id"
                v => Enum.Parse<PwdAlgorithm>(v));  // DB → C#
        e.Property(c => c.MustChange).HasColumnName("must_change");
        e.Property(c => c.LastChangedAt).HasColumnName("last_changed_at");
        e.Property(c => c.CreatedAt).HasColumnName("created_at");
    }
}