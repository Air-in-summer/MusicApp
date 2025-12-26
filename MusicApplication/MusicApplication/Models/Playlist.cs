using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicApplication.Models
{
    public partial class Playlist : ObservableObject
    {
        [ObservableProperty]
        private int playlistId;
        [ObservableProperty]
        private int userId;
        [ObservableProperty]
        private string? playlistName;
        [ObservableProperty]
        private DateTime createdAt;

        public bool IsSelected { get; set; } = false;
    }
}
