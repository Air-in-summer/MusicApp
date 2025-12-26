using MusicAppBackend.Data;
using MusicAppBackend.Repositories;
using MusicAppBackend.Models;
using Microsoft.AspNetCore.Mvc;
namespace MusicAppBackend.Services
{
    // service hỗ trợ gọi repo để thao tác với db và xử lý logic 
    public interface IPlaylistTracksService
    {
        Task<IActionResult> AddTrackToPlaylistAsync(AddTrackToPlaylistDto dto);
        Task<bool> DeleteTrackFromPlaylistAsync(int playlistId, int trackId);
        Task<IEnumerable<Track>> GetTracksByPlaylistIdAsync(int playlistId);
    }
    public class PlaylistTracksService : IPlaylistTracksService
    {
        private readonly IPlaylistTracksRepository _playlistTracksRepository;
        public PlaylistTracksService(IPlaylistTracksRepository playlistTracksRepository)
        {
            _playlistTracksRepository = playlistTracksRepository;
        }
        public async Task<IEnumerable<Track>> GetTracksByPlaylistIdAsync(int playlistId)
        {
            // Implement logic to retrieve tracks by playlist ID
            // This is a placeholder implementation
            var tracks = await _playlistTracksRepository.GetTracksByPlaylistIdAsync(playlistId);
            return tracks;
        }

        public async Task<IActionResult> AddTrackToPlaylistAsync(AddTrackToPlaylistDto dto)
        {
            // Implement logic to add a track to a playlist
            // This is a placeholder implementation
            if (dto == null || dto.PlaylistId <= 0 || dto.TrackId <= 0)
            {
                return new BadRequestObjectResult("Invalid input data.");
            }
            // Assuming _playlistTracksRepository has a method to add a track to a playlist
            var playlistTrack = new PlaylistTrack
            {
                PlaylistId = dto.PlaylistId,
                TrackId = dto.TrackId,
                AddedAt = DateTime.UtcNow
            };
            await _playlistTracksRepository.AddTrackToPlaylistAsync(playlistTrack);
            return new OkObjectResult(new { message = "Track added to playlist successfully.", playlistTrack });
        }

        public async Task<bool> DeleteTrackFromPlaylistAsync(int playlistId, int trackId)
        {
            bool response = await _playlistTracksRepository.DeleteTrackFromPlaylistAsync(playlistId, trackId);
            return response;
        }
    }
}
