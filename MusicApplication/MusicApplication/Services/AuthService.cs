using Microsoft.Maui.ApplicationModel.Communication;
using MusicApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MusicApplication.Services
{

    public class AuthResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
	// Lớp dịch vụ quản lý các thao tác liên quan đến xác thực người dùng.
	// Xử lý giao tiếp với Backend cho các nghiệp vụ: Đăng ký, Đăng nhập, Đổi mật khẩu và Làm mới Token.
    public class AuthService
    {
        private readonly HttpClient httpClient = ServiceHelper.GetService<HttpClient>();
        
	// Đăng ký tài khoản người dùng mới.
        public async Task<AuthResult> RegisterAsync(string username, string email, string password, string confirmPassword)
        {
            if(string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
            {
                return new AuthResult { Success = false, ErrorMessage = "Không được bỏ trống các trường." };
            }
            if (password != confirmPassword)
            {
                return new AuthResult { Success = false, ErrorMessage = "Mật khẩu không khớp." };
            }
            var payload = new
            {
                Username = username,
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/auth/register", content);

            return response.IsSuccessStatusCode
                ? new AuthResult { Success = true }
                : new AuthResult { Success = false, ErrorMessage = "Registration failed." };
        }

	// Xác thực thông tin đăng nhập và lưu trữ Token vào hệ thống lưu trữ bảo mật (SecureStorage).
        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            if(string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return new AuthResult { Success = false, ErrorMessage = "Email và mật khẩu không được để trống." };
            }
            var payload = new
            {
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (result != null && !string.IsNullOrEmpty(result.Token) && result.User != null)
                {
                    await SecureStorage.SetAsync("token", result.Token);
                    if (!string.IsNullOrEmpty(result.RefreshToken))
                    {
                        await SecureStorage.SetAsync("refreshToken", result.RefreshToken);
                    }
                    await SecureStorage.SetAsync("userID", result.User.UserId.ToString());
                    await SecureStorage.SetAsync("username", result.User.Username ?? string.Empty);
                    await SecureStorage.SetAsync("email", result.User.Email ?? string.Empty);
                    await SecureStorage.SetAsync("coverImage", result.User.CoverImage ?? string.Empty);
                    await SecureStorage.SetAsync("role", result.User.Role ?? "user");
                    
                    return new AuthResult { Success = true };
                }
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return new AuthResult { Success = false, ErrorMessage = errorMessage };
        }

	// Thực hiện yêu cầu cấp lại Access Token mới từ Backend dựa trên Refresh Token hiện tại.
	// Phương thức này thường được gọi tự động bởi AuthInterceptor khi phát hiện lỗi 401.
        public async Task<bool> RefreshTokenAsync()
        {
            try
            {
                var refreshToken = await SecureStorage.GetAsync("refreshToken");
                if (string.IsNullOrEmpty(refreshToken)) return false;

                var payload = new { RefreshToken = refreshToken };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gọi API refresh trên Backend
                var response = await httpClient.PostAsync("api/auth/refresh", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<LoginResponse>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    
                    if (result != null && !string.IsNullOrEmpty(result.Token))
                    {
                        // Lưu lại cặp Token mới
                        await SecureStorage.SetAsync("token", result.Token);
                        if (!string.IsNullOrEmpty(result.RefreshToken))
                        {
                            await SecureStorage.SetAsync("refreshToken", result.RefreshToken);
                        }
                        return true; // Báo hiệu đã làm mới thành công
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi Refresh Token: {ex.Message}");
                return false;
            }
        }

        public async Task<AuthResult> ChangePassword(string oldPassword, string newPassword)
        {
            var userEmail = SecureStorage.GetAsync("email").Result;
            var payload = new
            {
                Email = userEmail,
                Password = oldPassword
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var newPayload = new
                {
                    Email = userEmail,
                    Password = newPassword
                };
                var newJson = JsonSerializer.Serialize(newPayload);
                var newContent = new StringContent(newJson, Encoding.UTF8, "application/json");
                var newResponse = await httpClient.PostAsync("api/auth/changepass", newContent);

                return newResponse.IsSuccessStatusCode
                    ? new AuthResult { Success = true }
                    : new AuthResult { Success = false, ErrorMessage = "Không thể đổi mật khẩu." };
            }
            else
            {
                return new AuthResult { Success = false, ErrorMessage = "Mật khẩu không đúng" };
            }
        } 
    }

    public class LoginResponse
    {
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? CoverImage { get; set; }
        public string? Role { get; set; }
    }

}
