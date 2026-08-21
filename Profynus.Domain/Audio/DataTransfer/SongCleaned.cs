using Profynus.Domain.Audio.Dynamic;

namespace Profynus.Domain.Audio.DataTransfer;

public class SongCleaned
{
    public Guid Id { get; set; }
    public string SongName { get; set; }
    public string ArtistName { get; set; }
    public string AlbumName { get; set; }
    public string Genre { get; set; }
    public int ReleaseYear { get; set; }
    public int DurationSeconds { get; set; }
    public string ThumbnailUrl { get; set; }
    public string AudioUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}