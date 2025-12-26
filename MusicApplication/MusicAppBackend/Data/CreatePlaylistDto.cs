namespace MusicAppBackend.Data
{
    // dto hỗ trợ tạo playlist 
    public class CreatePlaylistDto
    {
        public int UserId { get; set; }
        public string PlaylistName { get; set; } = string.Empty;
    }
}
