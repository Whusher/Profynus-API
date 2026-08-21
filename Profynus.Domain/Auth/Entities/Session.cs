namespace Profynus.Domain.Auth.Entities;

public class Session
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid DeviceId { get; set; }
    public string AccessTokenHash { get; set; } = default!;
    public string RefreshTokenHash { get; set; } = default!;
    public DateTimeOffset AccessExpiresAt { get; set; }
    public DateTimeOffset RefreshExpiresAt { get; set; }
    public string? IpAddress { get; set; }
    public string? GeoCountry { get; set; }
    public string? GeoCity { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUsedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }
    public string? RevokeReason { get; set; }
    // Cross-Entities
    public User.Entities.User User { get; set; } = default!;
    public Device Device { get; set; } = default!;
}