using Profynus.Domain.Auth.Enums;

namespace Profynus.Domain.Audio.Entities;

public class Song
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Core identity
    public string SongName { get; set; }
    public string ArtistName { get; set; }
    public string AlbumName { get; set; }
    public string Genre { get; set; }
    public int ReleaseYear { get; set; }
    public int DurationSeconds { get; set; }         // track length in seconds
    public string Language { get; set; }             // 2 letters ISO 639-1 language code
    
    // Source / storage
    public string SourceProvider { get; set; }      // e.g. 'spotify', 'youtube', 's3'
    public string SourceId {get; set;}              //  provider-specific ID or S3 key
    public string FileFormat { get; set; }          //  'mp3', 'flac', 'aac', etc.
    public long FileSizeBytes { get; set; }
    
    // Flexible extra attributes (BPM, key, mood, ISRC, explicit flag, cover art URL…)
    public string Metadata {get; set;}
    
    // Timestamps
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}