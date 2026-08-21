using Profynus.Domain.Auth.Entities;

namespace Profynus.Domain.User.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = default!;
    public string? Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool IsVerified { get; set; }
    public bool IsActive { get; set; }
    public bool IsLocked { get; set; }
    public short FailedLoginCount { get; set; }
    public DateTimeOffset? LockedUntil { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public Credential? Credential { get; set; }
    // Cross-Entitites
    public ICollection<Device> Devices { get; set; } = [];
    public ICollection<Session> Sessions { get; set; } = [];
}