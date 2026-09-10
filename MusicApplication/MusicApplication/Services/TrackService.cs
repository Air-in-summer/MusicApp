using MusicApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;

namespace MusicApplication.Services
{
	// Lớp dịch vụ chịu trách nhiệm giao tiếp với Backend API để thực hiện các thao tác CRUD đối với bài hát (Track).
	// Cung cấp các phương thức truy vấn danh sách, tìm kiếm, tải lên và xóa bài hát.
    public class TrackService
    {
        private readonly HttpClient httpClient;

        public TrackService()
        {
            httpClient = ServiceHelper.GetService<HttpClient>();
        }

        public async Task<List<Track>> GetTracksAsync()
        {
            try
            {
                var response = await httpClient.GetAsync("api/tracks");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Track>>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Track>();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Track>();
                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return new List<Track>();
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, rethrowing, etc.)
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<Track>();
            }
        }

        public async Task<List<Track>> GetTracksByUserIdAsync(int userId)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/tracks/{userId}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Track>>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Track>();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Track>();
                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return new List<Track>();
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, rethrowing, etc.)
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<Track>();
            }
        }

        public async Task<bool> UploadTrackAsync(MultipartFormDataContent uploadContent)
        {
            try
            {
                var response = await httpClient.PostAsync("api/tracks/upload", uploadContent);
                if (response.IsSuccessStatusCode)
                {
                    return true; // Upload successful
                }
                else
                {
                    Console.WriteLine($"Upload failed: {response.StatusCode} - {response.ReasonPhrase}");
                    return false; // Upload failed
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Upload failed: {ex.Message}");
                return false; // Upload failed
            }
        }

        public async Task<List<Track>> SearchTracksAsync(string query)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/tracks/search?query={Uri.EscapeDataString(query)}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Track>>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Track>();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Track>();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return new List<Track>();
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, rethrowing, etc.)
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<Track>();
            }
        }

        public async Task<List<Track>> GetTracksByPlaylistIdAsync(int playlistId)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/playlisttracks/{playlistId}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<Track>>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Track>();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<Track>();
                }

            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return new List<Track>();
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, rethrowing, etc.)
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<Track>();
            }
        }


        public async Task DeleteTrackAsync(int trackId)
        {
            var response = await httpClient.DeleteAsync($"api/Tracks/{trackId}");
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Playlist delete successfully: {response.StatusCode}");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }

        public async Task<bool> GetTrackByTrackIdAsync(int trackId)
        {
            var response = await httpClient.GetAsync($"api/Tracks/id/{trackId}");
            if(response.IsSuccessStatusCode) { return true; }
            else return false;
        }
    }
}
