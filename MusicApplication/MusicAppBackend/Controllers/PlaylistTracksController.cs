using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MusicAppBackend.Data;
using MusicAppBackend.Models;
using MusicAppBackend.Services;
using System.Threading.Tasks;

namespace MusicAppBackend.Controllers
{
    // controller kiểm soát các track trong playlist 
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistTracksController : ControllerBase
    {
        private readonly IPlaylistTracksService _playlistTrackService;

        public PlaylistTracksController(IPlaylistTracksService playlistTrackService)
        {
            _playlistTrackService = playlistTrackService;
        }

        // endpoint để lấy thông tin track trong playlist 
        // tham số truyền vào gồm playlistId 
        [HttpGet]
        [Route("{playlistId}")]
        public async Task<IActionResult> GetTracksByPlaylistId(int playlistId)
        {
            try
            {
                var tracks = await _playlistTrackService.GetTracksByPlaylistIdAsync(playlistId);
                return Ok(tracks);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving tracks: {ex.Message}");
            }
        }

        // endpoint để thêm track vào playlist
        [HttpPost]
        public async Task<IActionResult> AddTrackToPlaylist([FromBody] AddTrackToPlaylistDto dto)
        {
            try
            {
                // kiểm tra dữ liệu đầu vào
                if (dto == null || dto.PlaylistId <= 0 || dto.TrackId <= 0)
                {
                    return BadRequest("Invalid input data.");
                }

                // gọi service để thêm track vào playlist
                var playlistTrack = await _playlistTrackService.AddTrackToPlaylistAsync(dto);
                return Ok(new { message = "Track added to playlist successfully.", playlistTrack });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error adding track to playlist: {ex.Message}");
            }
        }

        // endpoint để xóa track khỏi playlist
        [HttpDelete]
        public async Task<IActionResult> DeleteTrackFromPlaylist(int playlistId, int trackId)
        {
            var response = await _playlistTrackService.DeleteTrackFromPlaylistAsync(playlistId, trackId);
            if (response)
            {
                return NoContent();
            }
            else
            {
                return NotFound();
            }
        }
    }
}
