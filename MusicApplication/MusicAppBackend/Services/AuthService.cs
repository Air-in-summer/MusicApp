using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicAppBackend.Data;
using MusicAppBackend.Models;
using System.Threading.Tasks;

namespace MusicAppBackend.Services
{
    // interface này định nghĩa các phương thức liên quan đến xác thực người dùng
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string username, string email, string password);
        Task<AuthResult> LoginAsync(string email, string password);
        Task<AuthResult> RefreshAccessTokenAsync(string oldRefreshToken); // Thêm hàm đổi Token
        Task<AuthResult> ChangePasswordAsync(string email, string newPassword);
    }

    // kết quả trả về từ các thao tác xác thực
    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; } // Thêm trường RefreshToken
        public User? User { get; set; }
    }

    // AuthService thực hiện các thao tác xác thực người dùng
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Đăng ký người dùng mới
        // Kiểm tra xem email đã tồn tại chưa, nếu có thì trả về lỗi
        // Nếu không thì tạo người dùng mới và lưu vào cơ sở dữ liệu
        public async Task<AuthResult> RegisterAsync(string username, string email, string password)
        {
            if (_context.Users.Any(u => u.Email == email))
                return new AuthResult { Success = false, ErrorMessage = "Email đã tồn tại." };

            //hash + salt mật khẩu
            string passwordHash;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                passwordHash = System.Convert.ToBase64String(hash);
            }

            //tạo người dùng mới và lưu vào cơ sở dữ liệu
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash, 
                PasswordSalt = passwordHash,
                CreatedAt = DateTime.UtcNow,
                Role = "user",
                CoverImage = ""
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return new AuthResult { Success = true };
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Email hoặc mật khẩu không đúng." };

            // Hash mật khẩu nhập vào để so sánh
            string passwordHash;
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                passwordHash = System.Convert.ToBase64String(hash);
            }
            if (user.PasswordHash != passwordHash)
                return new AuthResult { Success = false, ErrorMessage = "Email hoặc mật khẩu không đúng." };

            // Tạo JWT token và Refresh Token
            JwtHelper jwtHelper = new JwtHelper(_configuration);
            var token = jwtHelper.GenerateToken(user);
            var refreshToken = jwtHelper.GenerateRefreshToken();

            // Gán Refresh Token vào cơ sở dữ liệu
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // Thời gian hết hạn của Refresh Token (7 ngày)
            await _context.SaveChangesAsync();

            // Ẩn thông tin nhạy cảm trước khi trả về
            user.PasswordHash = null;
            user.PasswordSalt = null;
            return new AuthResult { Success = true, Token = token, RefreshToken = refreshToken, User = user };
        }

        // Xử lý cấp lại Access Token mới dựa trên Refresh Token
        public async Task<AuthResult> RefreshAccessTokenAsync(string oldRefreshToken)
        {
            // Tìm người dùng sở hữu Refresh Token tương ứng
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == oldRefreshToken);
            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Refresh Token không tồn tại hoặc không hợp lệ." };

            // Kiểm tra thời hạn của Refresh Token
            if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return new AuthResult { Success = false, ErrorMessage = "Refresh Token đã hết hạn. Vui lòng đăng nhập lại." };

            // Khởi tạo cặp Token mới 
            JwtHelper jwtHelper = new JwtHelper(_configuration);
            var newToken = jwtHelper.GenerateToken(user);
            var newRefreshToken = jwtHelper.GenerateRefreshToken();

            // Cập nhật Database
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            user.PasswordHash = null;
            user.PasswordSalt = null;

            return new AuthResult { Success = true, Token = newToken, RefreshToken = newRefreshToken, User = user };
        }

        public async Task<AuthResult> ChangePasswordAsync(string email, string newPassword)
        {
            User? user = null;

            
            if (!string.IsNullOrWhiteSpace(email))
                user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
                return new AuthResult { Success = false, ErrorMessage = "Không tìm thấy người dùng." };

            
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(newPassword);
                var hash = sha.ComputeHash(bytes);
                user.PasswordHash = System.Convert.ToBase64String(hash);
            }

            await _context.SaveChangesAsync();

            return new AuthResult { Success = true };
        }

    }
}
