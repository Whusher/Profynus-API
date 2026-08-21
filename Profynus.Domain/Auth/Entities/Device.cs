using Profynus.Domain.Auth.Enums;

namespace Profynus.Domain.Auth.Entities;

public class Device
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public ClientType ClientType { get; set; }
    public string? Platform { get; set; }
    public string? Os { get; set; }
    public string? OsVersion { get; set; }
    public string? AppVersion { get; set; }
    public string? UaHash { get; set; }
    public Dictionary<string, object>? Fingerprint { get; set; }
    public string? MacHash { get; set; }
    public string? Name { get; set; }
    public bool IsTrusted { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset FirstSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastSeenAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }
    // Cross-Entities
    public User.Entities.User User { get; set; } = default!;
    public ICollection<Session> Sessions { get; set; } = [];
}