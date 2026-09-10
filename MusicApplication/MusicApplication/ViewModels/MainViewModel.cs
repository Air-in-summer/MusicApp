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
	// ViewModel chịu trách nhiệm quản lý dữ liệu cho màn hình chính (HomePage).
	// Xử lý logic tải danh sách bài hát nổi bật và cập nhật giao diện thông qua cơ chế Data Binding.
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly TrackService trackService;
        public ObservableCollection<Track> listTrack { get; set; } = new ObservableCollection<Track>();
        public ObservableCollection<Track> Tracks
        {
            get => listTrack;
            set
            {
                listTrack = value;
                OnPropertyChanged(nameof(Track));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor injection
        public MainViewModel()
        {
            trackService = ServiceHelper.GetService<TrackService>();
        }

        public async Task LoadSongs()
        {
            var responseTrack = await trackService.GetTracksAsync();
            Tracks.Clear();
            foreach (var item in responseTrack)
            {
                Tracks.Add(item);
                Console.WriteLine($"Loaded song: {item.Title} - {item.Artist}");
            }
            OnPropertyChanged(nameof(listTrack));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            Console.WriteLine($"📌 OnPropertyChanged được gọi cho: {name}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
