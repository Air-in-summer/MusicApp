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
    public class DownloadService
    {
        private static SQLiteAsyncConnection _database;
        private readonly ConnectivityService connectivityService;

        //khởi tạo db nếu chưa tồn tại 
        public static async Task InitAsync()
        {
            if (_database != null) { 
                
                return;
            }

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "downloads.db");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<DownloadedTrack>();
        }

        // lấy các track được người dùng đó tải 
        // do 1 app có nhiều userID đăng nhập
        // ai là người đăng nhập thì truy vấn theo userID của người đó 
        public static async Task<List<DownloadedTrack>> GetDownloadsByUserAsync(int userId)
        {
            await InitAsync();
            return await _database.Table<DownloadedTrack>().Where(t => t.UserId == userId).ToListAsync();
        }

        public async Task<bool> GetDownloadedTrack(int userId, int trackId)
        {
            await InitAsync();
            var track = await _database.Table<DownloadedTrack>()
                                        .Where(t => t.UserId == userId && t.TrackId == trackId)
                                        .FirstOrDefaultAsync();
            return track != null;
        }

        // thêm bản ghi vào local db khi người dùng tải xuống 1 track
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
                ExpirationTimeTicks = DateTime.UtcNow.AddDays(7).Ticks, // ví dụ: hết hạn sau 7 ngày
                TrackStatus = (int)TrackStatus.Available
            };

            await _database.InsertAsync(downloadedTrack);
        }

        // xóa track 
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
        // xóa track hết hạn 
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

        //tải bài hát từ URL về local
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

        public static async Task UpdateExpirationAsync(DownloadedTrack track)
        {
            await InitAsync();
            await _database.UpdateAsync(track);
        }

        // các thao tác với downloadpage và downloadviewmodel
        public async Task<IEnumerable<DownloadedTrack>> GetDownloadsTracksAsync()
        {
            var userIdString = SecureStorage.GetAsync("userID").Result;
            int userId = int.Parse(userIdString);    

            // lấy tất cả track đã download ứng với userId này
            // kiểm tra từng track 
            // nếu online thì kiểm tra track còn trên server k => k còn thì xóa , còn thì cập nhật Expiraion là now
            // các track k đủ điều kiện bị remove khỏi localTracks
            // nếu k onl => kiểm tra track.ExpirationTime > DateTime.UtcNow? => status = Unavailable
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
