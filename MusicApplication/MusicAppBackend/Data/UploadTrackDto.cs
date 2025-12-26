using System.ComponentModel.DataAnnotations;

namespace MusicAppBackend.Data
{
    // DTO gồm có kiểu File hỗ trợ upload track 
    public class UploadTrackDto
    {
        [Required]
        public IFormFile File { get; set; }

        public string Title { get; set; } = string.Empty;

        public int UserId { get; set; }
        public string Artist { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }
}
