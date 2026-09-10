using MusicAppBackend.Data;
using MusicAppBackend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
namespace MusicAppBackend.Repositories
{
    // thao tác với db 
    public interface IPlaylistTracksRepository
    {
        Task AddTrackToPlaylistAsync(PlaylistTrack playlistTrack);
        Task<bool> DeleteTrackFromPlaylistAsync(int playlistId, int trackId);

        // Define methods for playlist tracks operations, e.g., AddTrackToPlaylist, GetTracksByPlaylist, etc.
        Task<IEnumerable<Track>> GetTracksByPlaylistIdAsync(int playlistId);
    }
    public class PlaylistTracksRepository : IPlaylistTracksRepository
    {
        private readonly AppDbContext _context;
        public PlaylistTracksRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Track>> GetTracksByPlaylistIdAsync(int playlistId)
        {
            // Lấy track cho một người dùng cụ thể
            var tracks = await _context.PlaylistTracks
                .Where(pt => pt.PlaylistId == playlistId)
                .OrderBy(pt => pt.AddedAt)
                .Select(pt => pt.Track)
                .ToListAsync();
            return tracks;
        }

        public async Task AddTrackToPlaylistAsync(PlaylistTrack playlistTrack)
        {
            // Thêm 1 track mới vào playlist
            _context.PlaylistTracks.Add(playlistTrack);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteTrackFromPlaylistAsync(int playlistId, int trackId)
        {
            var entry = await _context.PlaylistTracks.FindAsync(playlistId, trackId);
            if (entry == null)
            {
                return false;
            }
            else
            {
                _context.PlaylistTracks.Remove(entry);
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }
}
