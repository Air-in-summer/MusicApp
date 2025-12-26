using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace MusicApplication.Models
{
    public partial class PlaylistTrack : ObservableObject
    {
        [ObservableProperty]
        private int playlistId;
        [ObservableProperty]
        private int trackId;
        [ObservableProperty]
        private DateTime addedAt;
    }
}
