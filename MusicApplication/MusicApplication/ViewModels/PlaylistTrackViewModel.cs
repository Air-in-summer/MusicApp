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
    // Lớp ViewModel quản lý việc hiển thị danh sách các bản nhạc thuộc về một danh sách phát cụ thể.
    public class PlaylistTrackViewModel
    {
        private readonly TrackService trackService;
        public ObservableCollection<Track> Tracks { get; set; } = new ObservableCollection<Track>();
        public event PropertyChangedEventHandler PropertyChanged;
        // Constructor injection
        public PlaylistTrackViewModel()
        {
            trackService = ServiceHelper.GetService<TrackService>();
            
        }
        
        // Tải toàn bộ bản nhạc thuộc về danh sách phát được chỉ định thông qua định danh và đồng bộ hóa lên Collection.
        public async Task LoadTracksByPlaylistIdAsync(int playlistId)
        {
            var responseTrack = await trackService.GetTracksByPlaylistIdAsync(playlistId);
            Tracks.Clear();
            foreach (var item in responseTrack)
            {
                Tracks.Add(item);
                Console.WriteLine($"Loaded track: {item.Title} - {item.Artist}");
            }
            OnPropertyChanged(nameof(Tracks));
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            Console.WriteLine($"📌 OnPropertyChanged được gọi cho: {name}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
