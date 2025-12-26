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
                    OnPropertyChanged(nameof(IsNotSelectingPlaylists)); // cũng phải cập nhật cái này
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

        public async Task CreatePlaylistAsync(string playlistName)
        {
            await playlistService.CreatePlaylistAsync(playlistName);
        }

        public async Task AddTrackToPlaylistAsync(int playlistId, int trackId)
        {
            await playlistService.AddTrackToPlaylistAsync(playlistId, trackId);
        }
    }
}
