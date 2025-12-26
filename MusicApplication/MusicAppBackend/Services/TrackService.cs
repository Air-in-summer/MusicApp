using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MusicAppBackend.Data;
using MusicAppBackend.Models;
using MusicAppBackend.Repositories;
namespace MusicAppBackend.Services
{
    // service hỗ trợ gọi repo để thao tác với db và xử lý logic 
    public interface ITrackService
    {
        Task<bool> DeleteTrackAsync(int trackId);

        // lấy tất cả các track
        Task<IEnumerable<Track>> GetAllTracksAsync();
        Task<IEnumerable<Track>> GetTrackByTrackIdAsync(int trackId);
        Task<IEnumerable<Track>> GetTracksByUserIdAsync(int userId);
        Task<IEnumerable<Track>> SearchTracksAsync(string query);

        // upload một track mới với tham số truyền vào là File
        Task<IActionResult> UploadTrackAsync(UploadTrackDto uploadTrackDto);

    }

    public class TrackService : ITrackService
    {
        private readonly ITrackRepository _trackRepository;
        private readonly S3Service _s3Service;
        public TrackService(ITrackRepository trackRepository, S3Service s3Service)
        {
            _trackRepository = trackRepository;
            _s3Service = s3Service;
        }

        public async Task<IEnumerable<Track>> GetAllTracksAsync()
        {
            return await _trackRepository.GetAllTracksAsync();
        }

        public async Task<IEnumerable<Track>> GetTracksByUserIdAsync(int userId)
        {
            return await _trackRepository.GetTrackByUserIdAsync(userId);
        }

        public async Task<IEnumerable<Track>> GetTrackByTrackIdAsync(int trackId)
        {
            return await _trackRepository.GetTrackByTrackIdAsync(trackId);
        }
        public async Task<IEnumerable<Track>> SearchTracksAsync(string query)
        {
            var result = await _trackRepository.SearchTracksAsync(query);
            return result;
        }

        public async Task<IActionResult> UploadTrackAsync(UploadTrackDto uploadTrackDto)
        {
            if (uploadTrackDto.File == null || uploadTrackDto.File.Length == 0)
            {
                return new BadRequestObjectResult("Invalid file.");
            }
            // Tạo tên file duy nhất
            var fileName = $"{Guid.NewGuid()}_{uploadTrackDto.File.FileName}";
            // Lưu file tạm thời
            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await uploadTrackDto.File.CopyToAsync(stream);
            }
            // Upload file lên AWS S3
            var fileUrl = await _s3Service.UploadFileAsync(filePath, $"audio/{fileName}");
            // Tạo đối tượng track và lưu vào DB
            var track = new Track
            {
                Title = uploadTrackDto.Title,
                UserId = uploadTrackDto.UserId,
                Artist = uploadTrackDto.Artist,
                Genre = uploadTrackDto.Genre,
                Duration = TimeSpan.Parse(uploadTrackDto.Duration),
                CoverImage = "",
                AudioUrl = fileUrl,
                CreatedAt = DateTime.UtcNow,
                Public = true // Mặc định là public, có thể thay đổi sau
            };
            await _trackRepository.AddTrackAsync(track);
            return new OkObjectResult(new { message = "Track uploaded successfully.", trackId = track.TrackId });
        }

        public async Task<bool> DeleteTrackAsync(int trackId)
        {
            string audioUrl = await _trackRepository.DeleteTrackAsync(trackId);
            if(audioUrl != string.Empty)
            {
                var uri = new Uri(audioUrl);
                var key = uri.AbsolutePath.TrimStart('/');
                var keyName = Uri.UnescapeDataString(key);
                await _s3Service.DeleteFileAsync(keyName);
                return true;
            }
            return false;
        }

        
    }
}