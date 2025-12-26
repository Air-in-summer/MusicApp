using System.Collections.Generic;
using System.Threading.Tasks;
using MusicAppBackend.Models;
using MusicAppBackend.Data;
using Microsoft.EntityFrameworkCore;

namespace MusicAppBackend.Repositories
{
    // thao tác với db 
    public interface ITrackRepository
    {
        Task<IEnumerable<Track>> GetAllTracksAsync();

        Task AddTrackAsync(Track track);
        Task<IEnumerable<Track>> SearchTracksAsync(string query);
        Task<IEnumerable<Track>> GetTrackByUserIdAsync(int userId);
        Task<string> DeleteTrackAsync(int trackId);
        Task<IEnumerable<Track>> GetTrackByTrackIdAsync(int trackId);
    }

    public class TrackRepository : ITrackRepository
    {
        private readonly AppDbContext _context;
        public TrackRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Track>> GetAllTracksAsync()
        {
            return await _context.Tracks.ToListAsync();
        }

        public async Task<IEnumerable<Track>> GetTrackByUserIdAsync(int userId)
        {
            return await _context.Tracks
                        .Where (track => track.UserId == userId)
                        .OrderByDescending (track => track.CreatedAt)
                        .ToListAsync();
        }

        public async Task<IEnumerable<Track>> GetTrackByTrackIdAsync(int trackId)
        {
            return await _context.Tracks
                        .Where(track => track.TrackId == trackId)
                        .OrderByDescending(track => track.CreatedAt)
                        .ToListAsync();
        }

        // thêm phương thức để lưu track mới
        public async Task AddTrackAsync(Track track)
        {
            _context.Tracks.Add(track);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Track>> SearchTracksAsync(string query)
        {
            // Tìm kiếm các track theo tiêu đề hoặc nghệ sĩ
            var result = await _context.Tracks
                .Where(t => t.Title.Contains(query) || t.Artist.Contains(query))
                .ToListAsync();
            // Trả về kết quả tìm kiếm
            return result;
        }

        public async Task<string> DeleteTrackAsync(int trackId)
        {
            var content = await _context.Tracks.FindAsync(trackId);
            if(content == null) return string.Empty;
            else
            {
                string audioUrl = content.AudioUrl;
                _context.Tracks.Remove(content);
                await _context.SaveChangesAsync();
                return audioUrl;
            }
        }
    }
}

