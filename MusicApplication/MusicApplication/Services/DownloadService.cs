using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MusicApplication.Models;
using SQLite;

namespace MusicApplication.Services
{
    // Cung cấp các phương thức quản lý tiến trình tải xuống, lưu trữ và truy xuất tệp âm thanh cục bộ cùng siêu dữ liệu trên cơ sở dữ liệu SQLite.
    public class DownloadService
    {
        private static SQLiteAsyncConnection _database;
        private readonly ConnectivityService connectivityService;

        // Khởi tạo kết nối cơ sở dữ liệu SQLite cục bộ và tạo bảng nếu chưa tồn tại. 
        public static async Task InitAsync()
        {
            if (_database != null) { 
                
                return;
            }

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "downloads.db");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<DownloadedTrack>();
        }

        // Truy xuất danh sách các bản nhạc đã tải xuống dựa trên định danh người dùng.
        // Do ứng dụng hỗ trợ nhiều tài khoản, dữ liệu được phân lập theo UserID.
        public static async Task<List<DownloadedTrack>> GetDownloadsByUserAsync(int userId)
        {
            await InitAsync();
            return await _database.Table<DownloadedTrack>().Where(t => t.UserId == userId).ToListAsync();
        }

        // Kiểm tra sự tồn tại của một bản nhạc đã tải xuống trong cơ sở dữ liệu cục bộ.
        public async Task<bool> GetDownloadedTrack(int userId, int trackId)
        {
            await InitAsync();
            var track = await _database.Table<DownloadedTrack>()
                                        .Where(t => t.UserId == userId && t.TrackId == trackId)
                                        .FirstOrDefaultAsync();
            return track != null;
        }

        // Chèn bản ghi mới chứa thông tin siêu dữ liệu của bản nhạc vào cơ sở dữ liệu cục bộ sau khi hoàn tất tải xuống.
        public async Task AddDownloadedTrackAsync(Track track, string filePath, int userId)
        {
            await InitAsync();
            var downloadedTrack = new DownloadedTrack
            {
                TrackId = track.TrackId,
                UserId = userId,
                Title = track.Title,
                Artist = track.Artist,
                Genre = track.Genre,
                DurationTicks = track.Duration.Ticks,
                LocalPath = filePath,
                DownloadedAtTicks = DateTime.UtcNow.Ticks,
                ExpirationTimeTicks = DateTime.UtcNow.AddDays(7).Ticks, // Ví dụ: thiết lập thời gian hết hạn sau 7 ngày
                TrackStatus = (int)TrackStatus.Available
            };

            await _database.InsertAsync(downloadedTrack);
        }

        // Xóa bản ghi siêu dữ liệu khỏi cơ sở dữ liệu và xóa tệp vật lý tương ứng khỏi hệ thống tệp.
        public static async Task DeleteDownloadedTrackAsync(int trackId, int userId)
        {
            await InitAsync();
            var item = await _database.Table<DownloadedTrack>()
                                    .Where(t => t.TrackId == trackId && t.UserId == userId)
                                    .FirstOrDefaultAsync();
            if (item != null)
            {
                if(File.Exists(item.LocalPath))
                    File.Delete(item.LocalPath);
                await _database.DeleteAsync(item);
            }
        }

        // Biến thể của hàm xóa bản nhạc tải xuống, sử dụng ngữ cảnh đối tượng (non-static).
        public async Task DeleteDownloadedTrackAsync2(int trackId, int userId)
        {
            await InitAsync();
            var item = await _database.Table<DownloadedTrack>()
                                    .Where(t => t.TrackId == trackId && t.UserId == userId)
                                    .FirstOrDefaultAsync();
            if (item != null)
            {
                if (File.Exists(item.LocalPath))
                    File.Delete(item.LocalPath);
                await _database.DeleteAsync(item);
            }
        }
        // Quét và loại bỏ hoàn toàn các tệp âm thanh cùng siêu dữ liệu đã vượt quá thời hạn lưu trữ khả dụng.
        public static async Task ClearExpiredTracksAsync()
        {
            await InitAsync();
            var now = DateTime.UtcNow;
            var expiredTracks = await _database.Table<DownloadedTrack>()
                                    .Where(t => t.ExpirationTimeTicks < now.Ticks)
                                    .ToListAsync();

            foreach (var track in expiredTracks)
            {
                if (File.Exists(track.LocalPath))
                    File.Delete(track.LocalPath);

                await _database.DeleteAsync(track);
            }
        }

        // Thực hiện tải tệp âm thanh từ URL thông qua giao thức HTTP và ghi dữ liệu luồng vào hệ thống tệp cục bộ.
        public async Task<string> DownloadFileAsync(string? audioUrl, string userFolderPath ,string fileName)
        {
            var httpClient = ServiceHelper.GetService<HttpClient>();
            var localPath = Path.Combine(userFolderPath, fileName);
            using (var stream = await httpClient.GetStreamAsync(audioUrl))
            using (var fileStream = File.Create(localPath))
            {
                await stream.CopyToAsync(fileStream);
            }

            return localPath;
        }

        // Cập nhật thông tin gia hạn lưu trữ cho bản nhạc trong cơ sở dữ liệu cục bộ.
        public static async Task UpdateExpirationAsync(DownloadedTrack track)
        {
            await InitAsync();
            await _database.UpdateAsync(track);
        }

        // Xử lý logic đồng bộ hóa và truy xuất toàn bộ danh sách bản nhạc đã tải của người dùng.
        public async Task<IEnumerable<DownloadedTrack>> GetDownloadsTracksAsync()
        {
            var userIdString = SecureStorage.GetAsync("userID").Result;
            int userId = int.Parse(userIdString);    

            // Lấy danh sách tệp cục bộ tương ứng với định danh người dùng.
            // Quá trình sẽ đối chiếu với trạng thái khả dụng từ máy chủ để gia hạn hoặc vô hiệu hóa.
            var localTracks = await DownloadService.GetDownloadsByUserAsync(userId);
            foreach (var track in localTracks.ToList())
            {
                bool isValid = track.ExpirationTime > DateTime.UtcNow;
                var connectivityService = ServiceHelper.GetService<ConnectivityService>();
                //var apiService = ServiceHelper.GetService<ApiService>();
                var isOnline = connectivityService.IsServerReachable; // apiService.IsApiReachable(); // thay thế bằng ping đên server 
                if (isOnline)
                {
                    // Kiểm tra track còn tồn tại trên server
                    var trackService = ServiceHelper.GetService<TrackService>();
                    bool isAvailableTrack = await trackService.GetTrackByTrackIdAsync(track.TrackId);
                                       
                    if (!isAvailableTrack)
                    {
                        // Track đã bị xóa → xóa khỏi local
                        await DownloadService.DeleteDownloadedTrackAsync(track.TrackId, userId);
                        if (File.Exists(track.LocalPath))
                            File.Delete(track.LocalPath);

                        localTracks.Remove(track); // loaij bo track k du điều kiện 
                        continue;
                    }

                    // Cập nhật ExpirationTime
                    track.ExpirationTimeTicks = DateTime.UtcNow.AddDays(7).Ticks;
                    await DownloadService.UpdateExpirationAsync(track);

                    track.TrackStatus = (int)TrackStatus.Available;
                    //string durationString = TimeSpan.FromTicks(track.DurationTicks).ToString(@"hh\:mm\:ss");
                    //track.Duration = TimeSpan.ParseExact(durationString, @"hh\:mm\:ss", null);

                }
                else  
                {
                    // không online 
                    //status = isValid ? "Có thể phát" : "Cần kết nối mạng để gia hạn";
                    if(isValid) { 
                        track.TrackStatus = (int) TrackStatus.Available;
                        //string durationString = TimeSpan.FromTicks(track.DurationTicks).ToString(@"hh\:mm\:ss");
                        //track.Duration = TimeSpan.ParseExact(durationString, @"hh\:mm\:ss", null);
                    }
                    else { track.TrackStatus = (int)TrackStatus.Unavailable; }
                }
                // Gán Duration trực tiếp từ DurationTicks
                //track.Duration = TimeSpan.FromTicks(track.DurationTicks);
            }

            return localTracks;
        }
    }
}
