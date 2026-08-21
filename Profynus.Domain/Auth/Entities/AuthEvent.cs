using Profynus.Domain.Auth.Enums;

namespace Profynus.Domain.Auth.Entities;

public class AuthEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? DeviceId { get; set; }
    
    // Navigation properties — EF uses these for ordering
    public User.Entities.User? User { get; set; }
    public Session? Session { get; set; }
    public Device? Device { get; set; }
    
    
    public AuthEventType EventType { get; set; }
    public EventStatus Status { get; set; }
    public string? IpAddress { get; set; }
    public string? FailureReason { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = [];
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
}