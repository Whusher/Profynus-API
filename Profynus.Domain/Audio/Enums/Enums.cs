namespace Profynus.Domain.Audio.Enums;

public enum DownloadStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Canceled
}
public enum DownloadPriority
{
    Low,
    Normal,
    High,
    Urgent
}

public enum UrlStatus
{
    Active,
    Expired,
    Revoked
}

public enum ListenEventType
{
    Play,
    Pause,
    Skip,
    Complete,
    Seek
}
 