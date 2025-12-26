using MusicAppBackend.Models;
using MusicAppBackend.Repositories;
using MusicAppBackend.Data;
using Microsoft.AspNetCore.Mvc;
namespace MusicAppBackend.Services
{
    // service hỗ trợ gọi repo để thao tác với db và xử lý logic 
    public interface IPlaylistService
    {
        Task<IActionResult> CreatePlaylistAsync(CreatePlaylistDto dto);
        Task<bool> DeletePlaylistAsync(int playlistId);

        // Define methods for playlist operations here
        Task<IEnumerable<Playlist>> GetPlaylistsByUserAsync(int userId);
    }
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;

        public PlaylistService(IPlaylistRepository playlistRepository)
        {
            _playlistRepository = playlistRepository;
        }
        public async Task<IEnumerable<Playlist>> GetPlaylistsByUserAsync(int userId)
        {
            // Implement logic to retrieve playlists by user ID
            // This is a placeholder implementation
            var result = await _playlistRepository.GetPlaylistsByUserAsync(userId);
            return result;
        }

        public async Task<IActionResult> CreatePlaylistAsync(CreatePlaylistDto dto)
        {
            // Implement logic to create a new playlist
            // This is a placeholder implementation
            var playlist = new Playlist
            {
                PlaylistName = dto.PlaylistName,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            };
            // Assuming _playlistRepository has a method to add a playlist
            await _playlistRepository.AddPlaylistAsync(playlist);
            return new OkObjectResult(new { message = "Playlist created successfully", playlist });
        }

        public async Task<bool> DeletePlaylistAsync(int playlistId)
        {
            var response = await _playlistRepository.DeletePlaylistAsync(playlistId);
            return response;
        }
    }
}
