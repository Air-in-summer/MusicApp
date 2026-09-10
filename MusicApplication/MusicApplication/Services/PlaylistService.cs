using MusicApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MusicApplication.Services
{
    // Cung cấp các dịch vụ tương tác với API máy chủ để thực hiện thao tác CRUD trên danh sách phát của người dùng.
    public class PlaylistService
    {
        private readonly HttpClient httpClient;
        public PlaylistService()
        {
            httpClient = ServiceHelper.GetService<HttpClient>();
        }
        
        // Gọi API để truy xuất danh sách các playlist dựa trên định danh của người dùng hiện tại.
        public async Task<List<Playlist>> GetPlaylistsByIdAsync()
        {
            var userId = SecureStorage.GetAsync("userID").Result;     
            int UserID = int.Parse(userId);
            try
            {
                var response = await httpClient.GetAsync($"api/playlist/user/{UserID}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Playlist>>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Playlist>();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Playlist>();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return new List<Playlist>();
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, rethrowing, etc.)
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<Playlist>();
            }
        }

        // Gửi yêu cầu HTTP POST để khởi tạo một playlist mới trên hệ thống máy chủ.
        public async Task<bool> CreatePlaylistAsync(string playlistName)
        {
            var userId = SecureStorage.GetAsync("userID").Result;
            int UserID = int.Parse(userId);
            var playlist = new 
            {
                UserId = UserID,
                PlaylistName = playlistName
            };
            var response = await httpClient.PostAsJsonAsync("api/playlist", playlist);
            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Playlist '{playlistName}' created successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to create playlist: {response.StatusCode} - {response.ReasonPhrase}");
            }
            return response.IsSuccessStatusCode;
        }

        // Thực hiện liên kết một bản nhạc vào một playlist cụ thể thông qua API.
        public async Task AddTrackToPlaylistAsync(int playlistId, int trackId)
        {
            var track = new
            {
                PlaylistId = playlistId,
                TrackId = trackId
            };
            var response = await httpClient.PostAsJsonAsync("api/playlisttracks", track);    
            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Track with ID {trackId} added to playlist with ID {playlistId} successfully.");
            }
            else
            {
                Console.WriteLine($"Failed to add track to playlist: {response.StatusCode} - {response.ReasonPhrase}");
            }
        }

        // Gỡ bỏ liên kết của một bản nhạc khỏi playlist được chỉ định trên hệ thống máy chủ.
        public async Task DeleteTrackFromPlaylistAsync(int playlistId, int trackId)
        {
            var response = await httpClient.DeleteAsync($"api/PlaylistTracks?playlistId={playlistId}&trackId={trackId}");
            if(response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Track delete successfully: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }

        // Gửi yêu cầu định danh để xóa hoàn toàn một playlist khỏi hệ thống máy chủ.
        public async Task DeletePlaylistAsync(int playlistId)
        {
            var response = await httpClient.DeleteAsync($"api/Playlist/{playlistId}");
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Playlist delete successfully: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }

    }
}
