

using MusicAppBackend.Models;
using MusicAppBackend.Repositories;

namespace MusicAppBackend.Services
{
    // service hỗ trợ gọi repo để thao tác với db và xử lý logic 
    public interface IUserService
    {
        Task<IEnumerable<User>> SearchUsersAsync(string query);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<IEnumerable<User>> SearchUsersAsync(string query)
        {
            return await _userRepository.SearchUsersAsync(query);
        }
    }
}
