using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
namespace MusicApplication.Services
{
    public class TrackDeletedFromPlaylistMessage : ValueChangedMessage<int>
    {
        // Nếu cần truyền thêm dữ liệu, bạn có thể thêm thuộc tính ở đây
        public TrackDeletedFromPlaylistMessage(int trackId) : base(trackId)
        {
        }
    }

    public class PlaylistDeleteMessage : ValueChangedMessage<int> 
    {
        public PlaylistDeleteMessage(int playlistId) : base(playlistId) { }
    }

    public class TrackDeletedMessage : ValueChangedMessage<int>
    {
        public TrackDeletedMessage(int trackId) : base(trackId) { }
    }

    public class DownloadedTrackDeletedMessage : ValueChangedMessage<int>
    {
        public DownloadedTrackDeletedMessage(int trackId) : base(trackId) { }
    }

}
