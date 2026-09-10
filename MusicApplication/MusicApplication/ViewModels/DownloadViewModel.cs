using MusicApplication.Models;
using MusicApplication.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MusicApplication.ViewModels
{
    // Lớp ViewModel chịu trách nhiệm xử lý logic và liên kết dữ liệu cho giao diện quản lý các tệp âm thanh đã tải xuống cục bộ.
    public class DownloadViewModel
    {
        public ObservableCollection<DownloadedTrack> DownloadedTracks { get; set; } = new ObservableCollection<DownloadedTrack>();
        private readonly DownloadService downloadService;
        public event PropertyChangedEventHandler PropertyChanged;
        public DownloadViewModel()
        {
            downloadService = ServiceHelper.GetService<DownloadService>();
        }

        // Tải danh sách các tệp âm thanh từ bộ nhớ cục bộ vào Collection, đồng thời kích hoạt sự kiện cập nhật giao diện người dùng.
        public async Task LoadDownloadedTracks()
        {
            var downloadedTracks = await downloadService.GetDownloadsTracksAsync();
            DownloadedTracks.Clear();
            foreach (var item in downloadedTracks)
            {              
                DownloadedTracks.Add(item);
                Console.WriteLine($"Loaded song: {item.Title} - {item.Artist}");
            }
            OnPropertyChanged(nameof(DownloadedTracks));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            Console.WriteLine($"📌 OnPropertyChanged được gọi cho: {name}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
