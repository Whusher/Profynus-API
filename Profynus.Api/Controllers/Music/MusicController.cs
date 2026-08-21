using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Profynus.Application.DTO.Music;
using Profynus.Application.YTAudio;
using Profynus.Domain.Audio.Entities;
using YoutubeExplode.Playlists;

namespace Profynus.Api.Controllers.Music;
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MusicController : ControllerBase
{
    private readonly ILogger<MusicController> _logger;
    private readonly YTAudioService  _ytAudioService;

    public MusicController(ILogger<MusicController> logger, YTAudioService ytAudioService)
    {
        _logger = logger;
        _ytAudioService = ytAudioService;
    }
    
    
    #region Profynus Music

    [HttpGet("getPublicPlaylist")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPublicPlaylist(int pageNumber, int pageSize)
    {
        try
        {
            var result = await _ytAudioService.GetAllSongsAsync(pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return BadRequest(new {Error = "Error obtaining playlist" });
        }
    }
    
    [HttpGet("getPlaylists")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPlaylists()
    {
        return Ok();
    }

    [HttpPost("createPlaylist")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlaylist()
    {
        return Ok();
    }

    [HttpPost("addSongToPlaylist")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSongToPlaylist()
    {
        return Ok();
    }
    
    [HttpPatch("updatePlaylistData")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePlaylist()
    {
        return Ok();
    }


    [HttpDelete("deletePlaylist/{playlistId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeletePlaylist(string playlistId)
    {
        return Ok();
    }
    
    #endregion
    
    #region Profynus Subscription
    
    /// <summary>
    ///  Validate and obtain a token before proceed with download request 
    /// </summary>
    /// <returns></returns>
    [HttpPost("validateSong")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateSong()
    {
        return Ok();
    }

    [HttpPost("downloadYTSong")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadYTSong([FromBody] YTDownloadRequest request)
    {
        try
        {
            var response = await _ytAudioService.DownloadSongAsync(request.Url);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest("Error during download song");
        }
    }

    [HttpPost("downloadSong")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadSong()
    {
        return Ok();
    }
    
    #endregion
    
}