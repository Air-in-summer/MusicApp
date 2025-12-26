namespace MusicAppBackend.Data
{
    // DTo hỗ trợ thêm bài hát vào playlist 
    public class AddTrackToPlaylistDto
    {
        public int PlaylistId { get; set; }
        public int TrackId { get; set; }
    }
}
