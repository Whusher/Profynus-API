using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Profynus.Domain.User.Entities;

namespace Profynus.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> e)
    {
        // Table specification
        e.ToTable("users");
        
        // Primary Key specification
        e.HasKey(x => x.Id);
        
        // Table columns specification
        e.Property(x => x.Id)
            .HasColumnName("id");
        e.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();
        e.Property(x => x.Username)
            .HasColumnName("username")
            .HasMaxLength(100);
        e.Property(x => x.FirstName).HasColumnName("first_name");
        e.Property(x => x.LastName).HasColumnName("last_name");
        e.Property(x => x.IsVerified).HasColumnName("is_verified");
        e.Property(x => x.IsActive).HasColumnName("is_active");
        e.Property(x => x.IsLocked).HasColumnName("is_locked");
        e.Property(x => x.FailedLoginCount).HasColumnName("failed_login_count");
        e.Property(x => x.LockedUntil).HasColumnName("locked_until");
        e.Property(x => x.CreatedAt).HasColumnName("created_at");
        e.Property(x => x.UpdatedAt).HasColumnName("updated_at");
        e.Property(x => x.DeletedAt).HasColumnName("deleted_at");

        // Indexing specification
        e.HasIndex(x => x.Email)
            .IsUnique();
            // .HasFilter("deleted_at IS NULL");
        
        // Force Where statement in each query against the DB
        // e.HasQueryFilter(x => x.DeletedAt == null);
    }
}