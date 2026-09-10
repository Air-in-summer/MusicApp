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
    // controller kiểm soát track
    [Authorize] 
    [ApiController]
    [Route("api/[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly ITrackService _trackService;       

        public TracksController(ITrackService trackService)
        {
            _trackService = trackService;
        }

        // lấy tất cả track
        [HttpGet]
        public async Task<IActionResult> GetAllTracks()
        {
            var tracks = await _trackService.GetAllTracksAsync();
            return Ok(tracks);
        }

        //lấy track theo userId 
        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetTracksByUserId(int userId)
        {
            var tracks = await _trackService.GetTracksByUserIdAsync(userId);
            return Ok(tracks);
        }

        // lấy track theo trackId
        [HttpGet]
        [Route("id/{trackId}")]
        public async Task<IActionResult> GetTrackByTrackId(int trackId)
        {
            var track = await _trackService.GetTrackByTrackIdAsync(trackId);
            int numTrack = track.Count();
            if(numTrack != 0)
            {
                return Ok(track);
            }
            else
            {
                return NotFound();
            }
        }

        // endpoint để upload track mới với thông tin là File và thông tin khác của người tải lên
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(UploadTrackDto uploadTrackDto)
        {
            if (uploadTrackDto.File == null || uploadTrackDto.File.Length == 0)
            {
                return BadRequest("Invalid file.");
            }

            try
            {
                var track = await _trackService.UploadTrackAsync(uploadTrackDto);
                return Ok(new { message = "Tải lên thành công", track });
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi khi tải lên: {ex.Message}");
            }

        }


        // tìm kiếm track theo query 
        [HttpGet("search")]
        public async Task<IActionResult> SearchTracks([FromQuery] string query)
        {
            var result = await _trackService.SearchTracksAsync(query);

            return Ok(result);
        }

        // xoá 1 bài hát 
        [HttpDelete]
        [Route("{trackId}")]
        public async Task<IActionResult> DeleteTrack(int trackId)
        {
            var response = await _trackService.DeleteTrackAsync(trackId);
            if (response)
            {
                return NoContent();
            }
            else { return NotFound(); }
        }
    }
}