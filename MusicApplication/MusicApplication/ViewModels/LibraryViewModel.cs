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
    public class LibraryViewModel
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
        public LibraryViewModel()
        {
            trackService = ServiceHelper.GetService<TrackService>();
        }

        public async Task LoadUserTracks(int userId)
        {
            var responseTrack = await trackService.GetTracksByUserIdAsync(userId);
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

        public async Task<bool> UploadTrackAsync(MultipartFormDataContent content)
        {
            var response = await trackService.UploadTrackAsync(content);
            return response;
        }
    }
}
