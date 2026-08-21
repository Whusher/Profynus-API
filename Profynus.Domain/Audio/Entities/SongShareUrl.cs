using Profynus.Domain.Audio.Enums;

namespace Profynus.Domain.Audio.Entities;

public class SongShareUrl
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SongId { get; set; }
    public Guid CreatedBy { get; set; }
    public string Token { get; set; } // Auth token embedded in the URL (e.g. /stream/{song_id}?token={token})
    public UrlStatus Status { get; set; } // 'active', 'expired', 'revoked'
    public DateTimeOffset ExpiresAt { get; set; }
    public int MaxAccesses { get; set; }
    public int AccessCount { get; set; }
    public string IPWhitelist { get; set; }
    public string Metadata {get; set;}
    public DateTimeOffset CreatedAt { get; set; }
}