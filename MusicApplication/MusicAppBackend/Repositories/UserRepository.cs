using Microsoft.EntityFrameworkCore;
using MusicAppBackend.Data;
using MusicAppBackend.Models;
namespace MusicAppBackend.Repositories
{
    // thao tác với db 
    public interface IUserRepository
    {
        Task<IEnumerable<User>> SearchUsersAsync(string query);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _appDbContext;

        public UserRepository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string query)
        {
            var matchedUsers = await _appDbContext.Users
                                .Where(u => u.Username.Contains(query)) // hoặc dùng == nếu muốn chính xác
                                .ToListAsync();
            return matchedUsers;
        }
    }
}
