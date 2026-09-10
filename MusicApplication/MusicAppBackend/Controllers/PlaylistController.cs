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
    // controller kiểm soát các thao tác với playlist
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        // Lấy danh sách playlist theo userID
        [HttpGet]
        [Route("user/{userId}")]
        public async Task<IActionResult> GetPlaylistsByUser(int userId)
        {
            var playlists = await _playlistService.GetPlaylistsByUserAsync(userId);
            return Ok(playlists);
        }


        // tạo playlist mới 
        [HttpPost]
        public async Task<IActionResult> CreatePlaylist([FromBody] CreatePlaylistDto dto)
        {
            try
            {
                var playlist = await _playlistService.CreatePlaylistAsync(dto);
                return Ok(new { message = "Playlist created successfully", playlist });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating playlist: {ex.Message}");
            }
        }


        // xóa playlist với playlistId bất kỳ tồn tại 
        [HttpDelete("{playlistId}")]
        public async Task<IActionResult> DeletePlaylist(int playlistId)
        {
            var response = await _playlistService.DeletePlaylistAsync(playlistId);
            if(response)
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
