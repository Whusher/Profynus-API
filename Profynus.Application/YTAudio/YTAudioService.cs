using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Profynus.Infrastructure.Cache.Context;
using Profynus.Infrastructure.Persistence.Context;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Profynus.Application.Common.Exception;
using Profynus.Application.Common.Pagination;
using Profynus.Domain.Audio.DataTransfer;
using Profynus.Domain.Audio.Dynamic;
using Profynus.Domain.Audio.Entities;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace Profynus.Application.YTAudio;

public class YTAudioService(
    CacheService _redis,
    QueryDbContext _dbRead,
    MasterDbContext _dbWrite,
    IConfiguration _config,
    YoutubeClient _youtube,
    IHttpClientFactory _httpFactory,
    ILogger<YTAudioService> _logger
)
{
    // Main service URL
    private readonly string _serviceUrl = _config["ServerPaths:PublicUrl"] ?? "https://www.profynus.com";
    
    // Thumbnails & Common images
    private readonly string _physicalImagePath = _config["ServerPaths:Images:PhysicalPath"] ?? "";
    private readonly string _proxyImagePath = _config["ServerPaths:Images:ProxyPath"] ?? "";
    
    // Audio Files
    private readonly string _physicalAudioPath = _config["ServerPaths:AudioFiles:PhysicalPath"] ?? "";
    private readonly string _proxyAudioPath = _config["ServerPaths:AudioFiles:ProxyPath"] ?? "";
    private readonly int _audioPathDepth = 2;
    
    // Video Files
    private readonly string _physicalVideoPath = _config["ServerPaths:Videos:PhysicalPath"] ?? "";
    private readonly string _proxyVideoPath = _config["ServerPaths:Videos:ProxyPath"] ?? "";
    
    // Common Download Services
    public async Task<Song> DownloadSongAsync(string url)
    {
        try
        {
            
            // Main song object
            var newSong = new Song();
                // IMAGE THUMBNAIL
            string imageFileName = string.Empty;
            string thumbnailFilePath = string.Empty;
            string imageFilePath = string.Empty;
            
            
            // HTTP Client
            var http = _httpFactory.CreateClient("Profynus-Client");
            http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 16; RMX3842 Build/BP2A.250605.015; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/148.0.7778.217 Mobile Safari/537.36");
            
            // Get video information
            var videoInfo = await _youtube.Videos.GetAsync(url);
            
            // Get manifest
            var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoInfo.Id);
            
            // Get audio streams
            var audioStreamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            if (audioStreamInfo == null)
            {
                throw new Exception($"{url} does not contain audio stream");
            }
            
            // Start processing the secure storage
            var songSanitizedTitle = string.Join("_", videoInfo.Title.Split(Path.GetInvalidFileNameChars()));
            
            // Create folder structure
            string storagePath = GetStoragePath(newSong.Id.ToString());
            if (string.IsNullOrEmpty(storagePath))
            {
                throw new Exception($"Error creating storage path: {newSong.Id}");    
            }
            
            // Create the path
            var filePath = Path.Combine(storagePath, $"{songSanitizedTitle}.mp3");
            
            // Download audio stream
            await _youtube.Videos.Streams.DownloadAsync(audioStreamInfo, filePath);
            if (!System.IO.File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    $"Downloaded file not found: {filePath}");
            }
            long fileSize = new FileInfo(filePath).Length;
            
            // Store the cover image
            var thumb = videoInfo.Thumbnails?.OrderByDescending(t => t.Resolution.Width * t.Resolution.Height).FirstOrDefault();
            if (thumb != null)
            {
                // Store the image in the same storage space
                var thumbUri = new Uri(thumb.Url);
                var ext = Path.GetExtension(thumbUri.LocalPath);
                if (string.IsNullOrWhiteSpace(ext) || ext.Length > 5) ext = ".jpg";
                
                // Paths and cover image name
                imageFileName = $"{songSanitizedTitle}_cover.{ext}";
                thumbnailFilePath = GetStoragePath(newSong.Id.ToString(), "thumbnail");
                
                imageFilePath = Path.Combine(thumbnailFilePath, imageFileName);
                
                // Store the image
                using (var stream = await http.GetStreamAsync(thumbUri))
                using (var fs = System.IO.File.Create(imageFilePath))
                {
                    await stream.CopyToAsync(fs);
                }
            }
            
            // Fill song metadata
            var songMetadata = new
            {
                // Common data
                Title = videoInfo.Title,
                Description = videoInfo.Description,
                Author = videoInfo.Author?.Title,
                UploadDate = videoInfo.UploadDate,
                Duration = videoInfo.Duration?.ToString(),
                VideoId = videoInfo.Id.Value,
                SourceUrl = url,
                Image = imageFileName,
                SavedAt = DateTime.UtcNow,

                // Server handle data
                ThumbnailPublicPath = $"{_serviceUrl}{_proxyImagePath}/{newSong.Id.ToString()}/{imageFileName}",
                ThumbnailPhysicalPath = imageFilePath,
                SongPhysicalPath = filePath,
                SongPublicPath = $"{_serviceUrl}{_proxyAudioPath}/{newSong.Id.ToString()}/{songSanitizedTitle}.mp3",
            };
            
            // Fill master object to store in DB
            newSong.SongName = songSanitizedTitle;
            newSong.ArtistName = videoInfo.Author?.ChannelTitle ?? "Unknown";
            newSong.AlbumName = songMetadata.Title;
            newSong.Genre = "Unknown";
            newSong.ReleaseYear = videoInfo.UploadDate.Year;
            newSong.DurationSeconds = (int)(videoInfo.Duration?.TotalSeconds ?? 0);
            newSong.Language = "EN";
            newSong.SourceProvider = "youtube";
            newSong.SourceId = videoInfo.Id.Value;
            newSong.FileFormat = "mp3";
            newSong.FileSizeBytes = fileSize;
            newSong.Metadata = JsonSerializer.Serialize(songMetadata, new JsonSerializerOptions { WriteIndented = true });
            newSong.CreatedAt = DateTime.UtcNow;
            newSong.UpdatedAt = DateTime.UtcNow;
            
            // Save DB changes
            await _dbWrite.AddAsync(newSong);
            await _dbWrite.SaveChangesAsync();
            // Return the new song record downloaded successfully
            return newSong;
        }
        catch (Exception e)
        {
            _logger.LogError($"Error trying to download song: {e.Message}");
            throw new NotFoundException($"Error processing download for: {url}");
        }
    }

    public async Task<PaginatedResult<SongCleaned>> GetAllSongsAsync(
        int pageNumber = 1,
        int pageSize = 5)
    {
        var totalItems = await _dbRead.Songs.CountAsync();

        var songs = await _dbRead.Songs
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var songListNormalized = 
            songs.Select(x =>
                {
                    // Normalize Metadata DTO to avoid expose critical information
                    var commonMetadataObject = JsonSerializer.Deserialize<SongMetadata>(x.Metadata);
                    
                    return new SongCleaned()
                    {
                        Id = x.Id,
                        SongName = x.SongName,
                        ArtistName = x.ArtistName,
                        AlbumName = x.AlbumName,
                        Genre =  x.Genre,
                        ReleaseYear = x.ReleaseYear,
                        DurationSeconds = x.DurationSeconds,
                        ThumbnailUrl = commonMetadataObject.ThumbnailPublicPath,
                        AudioUrl = commonMetadataObject.SongPublicPath,
                        CreatedAt = x.CreatedAt,
                        UpdatedAt = x.UpdatedAt
                    };
                }
                
           ).ToList();
        
        
        return new PaginatedResult<SongCleaned>
        {
            Items = songListNormalized,
            TotalItems = totalItems,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize)
        };
    }
    private string GetStoragePath(string songUniqueId, string type="audio")
    {
        string storagePath = type switch
        {
            "thumbnail" => _physicalImagePath,
            "video" => _physicalVideoPath,
            _ => _physicalAudioPath
        };
        
        string path = Path.Combine(storagePath, songUniqueId);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }


}