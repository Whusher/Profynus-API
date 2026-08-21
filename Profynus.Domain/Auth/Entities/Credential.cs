using Profynus.Domain.Auth.Enums;

namespace Profynus.Domain.Auth.Entities;

public class Credential
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = default!;
    public PwdAlgorithm Algorithm { get; set; } = PwdAlgorithm.Argon2id;
    public bool MustChange {get; set;}
    public DateTimeOffset LastChangedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    // Cross-Entities
    public User.Entities.User User { get; set; } = default!;
}