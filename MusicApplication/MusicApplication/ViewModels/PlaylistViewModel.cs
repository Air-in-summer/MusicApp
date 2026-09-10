using MusicApplication.Models;
using MusicApplication.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MusicApplication.ViewModels
{
    // Lớp ViewModel xử lý các tác vụ quản lý danh sách phát của người dùng, bao gồm tạo, hiển thị và thêm bản nhạc.
    public class PlaylistViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool isSelectingPlaylists { get; set; } = false;
        public bool IsSelectingPlaylists
        {
            get => isSelectingPlaylists;
            set
            {
                if (isSelectingPlaylists != value)
                {
                    isSelectingPlaylists = value;
                    OnPropertyChanged(nameof(IsSelectingPlaylists));
                    OnPropertyChanged(nameof(IsNotSelectingPlaylists)); // Kích hoạt sự kiện cập nhật thuộc tính phái sinh
                }
            }
        }
        public bool IsNotSelectingPlaylists => !IsSelectingPlaylists;
        public Track SelectedTrack { get; set; }

        private readonly PlaylistService playlistService;

        
        public ObservableCollection<Playlist> Playlists { get; set; } = new ObservableCollection<Playlist>();

        

        // Constructor injection
        public PlaylistViewModel()
        {
            playlistService = ServiceHelper.GetService<PlaylistService>();
        }

        // Truy xuất toàn bộ danh sách phát từ máy chủ dựa trên thông tin định danh của người dùng hiện tại.
        public async Task LoadPlaylistsByIdAsync()
        {
            var loadPlaylists = await playlistService.GetPlaylistsByIdAsync();
            Playlists.Clear();
            foreach (var playlist in loadPlaylists)
            {
                Playlists.Add(playlist);
                Console.WriteLine($"Loaded playlist: {playlist.PlaylistName}");
            }
            OnPropertyChanged(nameof(Playlists));
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            Console.WriteLine($"📌 OnPropertyChanged được gọi cho: {name}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Gọi dịch vụ để khởi tạo một danh sách phát mới trên hệ thống máy chủ.
        public async Task CreatePlaylistAsync(string playlistName)
        {
            await playlistService.CreatePlaylistAsync(playlistName);
        }

        // Gọi dịch vụ để liên kết một bản nhạc cụ thể vào danh sách phát mục tiêu.
        public async Task AddTrackToPlaylistAsync(int playlistId, int trackId)
        {
            await playlistService.AddTrackToPlaylistAsync(playlistId, trackId);
        }
    }
}
