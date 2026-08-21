namespace Profynus.Domain.Audio.Dynamic;

public class SongMetadata
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? UploadDate { get; set; }
    public string? Duration { get; set; }
    public string? VideoId { get; set; }
    public string? SourceUrl { get; set; }
    public string? Image { get; set; }
    public string? SavedAt { get; set; }
    public string? ThumbnailPublicPath { get; set; }
    public string? ThumbnailPhysicalPath { get; set; }
    public string? SongPhysicalPath { get; set; }
    public string? SongPublicPath { get; set; }
}