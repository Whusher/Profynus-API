using Profynus.Domain.Auth.Enums;

namespace Profynus.Domain.Auth.Entities;

public class MfaConfig
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public MfaMethod Method {get; set;}
    public string SecretEnc {get; set;}
    public bool IsPrimary {get; set;}
    public bool IsVerified {get; set;}
    public DateTimeOffset CreatedAt {get; set;}
    public DateTimeOffset LastUsedAt {get; set;}
}