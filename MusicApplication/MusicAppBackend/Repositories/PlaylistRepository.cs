using MusicAppBackend.Models;
using MusicAppBackend.Data;
using Microsoft.EntityFrameworkCore;
namespace MusicAppBackend.Repositories
{
    // thao tác với db 
    public interface IPlaylistRepository
    {
        Task AddPlaylistAsync(Playlist playlist);
        Task<bool> DeletePlaylistAsync(int playlistId);

        // Define methods for playlist operations, e.g., GetPlaylists, AddPlaylist, etc.
        Task<IEnumerable<Playlist>> GetPlaylistsByUserAsync(int userId);
    }
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly AppDbContext _context;
        public PlaylistRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Playlist>> GetPlaylistsByUserAsync(int userId)
        {
            // Lấy danh sách phát cho một người dùng cụ thể
            return await _context.Playlists
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task AddPlaylistAsync(Playlist playlist)
        {
            // Thêm 1 playlist mới vào database
            _context.Playlists.Add(playlist);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeletePlaylistAsync(int playlistId)
        {
            var entry = await _context.Playlists.FindAsync(playlistId);
            if(entry == null)
            {
                return false;
            }
            else
            {
                _context.Playlists.Remove(entry);
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }
}
