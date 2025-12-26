using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicApplication.Models
{
    public partial class Track : ObservableObject
    {
        [ObservableProperty]
        private int trackId;
        [ObservableProperty]
        private string? title;
        [ObservableProperty]
        private int userId;
        [ObservableProperty]
        private string? artist;
        [ObservableProperty]
        private string? genre;
        [ObservableProperty]
        private TimeSpan duration;
        [ObservableProperty]
        private string? coverImage;
        [ObservableProperty]
        private string? audioUrl;
        [ObservableProperty]
        private DateTime createdAt;
        [ObservableProperty]
        private bool isPublic;
    }
}
