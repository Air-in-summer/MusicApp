using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;
namespace MusicApplication.Services
{
    // Lớp thông điệp định nghĩa sự kiện khi một bản nhạc bị xóa khỏi danh sách phát.
    public class TrackDeletedFromPlaylistMessage : ValueChangedMessage<int>
    {
        // Nếu cần truyền thêm dữ liệu, bạn có thể thêm thuộc tính ở đây
        public TrackDeletedFromPlaylistMessage(int trackId) : base(trackId)
        {
        }
    }

    // Lớp thông điệp định nghĩa sự kiện xóa toàn bộ danh sách phát.
    public class PlaylistDeleteMessage : ValueChangedMessage<int> 
    {
        public PlaylistDeleteMessage(int playlistId) : base(playlistId) { }
    }

    // Lớp thông điệp định nghĩa sự kiện khi một bản nhạc bị xóa.
    public class TrackDeletedMessage : ValueChangedMessage<int>
    {
        public TrackDeletedMessage(int trackId) : base(trackId) { }
    }

    // Lớp thông điệp định nghĩa sự kiện khi một bản nhạc đã tải xuống bị xóa khỏi bộ nhớ cục bộ.
    public class DownloadedTrackDeletedMessage : ValueChangedMessage<int>
    {
        public DownloadedTrackDeletedMessage(int trackId) : base(trackId) { }
    }

}
