using Profynus.Domain.Audio.Enums;

namespace Profynus.Domain.Audio.Entities;

public class ListeningEvent
{
    public Guid Id { get; set; }
    public Guid SongId { get; set; }
    public Guid UserId { get; set; }
    public Guid ShareUrlId { get; set; }
    public ListenEventType EventType { get; set; }
    public int PositionSecs { get; set; }
    public Guid SessionId { get; set; }
    public string ClientIp { get; set; }
    public string UserAgent { get; set; }
    public string CountryCode { get; set; }
    public string Metadata {get; set;}
    public DateTimeOffset OccurredAt { get; set; }
}