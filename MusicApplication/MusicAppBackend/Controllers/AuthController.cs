using Microsoft.AspNetCore.Mvc;
using MusicAppBackend.Models;
using MusicAppBackend.Services;
using System.Threading.Tasks;

namespace MusicAppBackend.Controllers
{
    // controller kiểm soát việc đăng ký, đăng nhập và đổi pass
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            // đầu vào của request gồm 3 thành phần
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Username, Email và Password không được để trống.");
            }

            var result = await _authService.RegisterAsync(request.Username!, request.Email!, request.Password!);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // đầu vào của request gồm 2 thành phần
            var result = await _authService.LoginAsync(request.Email, request.Password);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            
            // Trả về cả Access Token và Refresh Token cho Frontend
            return Ok(new { token = result.Token, refreshToken = result.RefreshToken, user = result.User });
        }

        // Endpoint nhận Refresh Token cũ để cấp cặp Token mới
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                return BadRequest("Refresh Token không được để trống.");

            var result = await _authService.RefreshAccessTokenAsync(request.RefreshToken);
            if (!result.Success)
                return Unauthorized(result.ErrorMessage); // Trả về 401 để báo Frontend phải đăng nhập lại

            return Ok(new { token = result.Token, refreshToken = result.RefreshToken, user = result.User });
        }


        [HttpPost("changepass")]
        public async Task<IActionResult> ChangePassword([FromBody] LoginRequest request)
        {
            // Kiểm tra điều kiện đầu vào
            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Mật khẩu mới không được để trống.");

            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Phải cung cấp Email.");

            var result = await _authService.ChangePasswordAsync(request.Email, request.Password);

            if (!result.Success)
                return BadRequest(result.ErrorMessage);

            return Ok("Đổi mật khẩu thành công.");
        }

    }

    // class hỗ trợ gửi request / trả về token
    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class LoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
    public class RefreshRequest
    {
        public string? RefreshToken { get; set; }
    }
}
