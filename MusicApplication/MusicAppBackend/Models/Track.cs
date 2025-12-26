using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace MusicAppBackend.Models
{
    [Table("track")]
    public class Track
    {
        [Key]
        [Column("track_id")]
        public int TrackId { get; set; }
        [Column("title")]
        public string? Title { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        [Column("artist")]
        public string? Artist { get; set; }
        [Column("genre")]
        public string? Genre { get; set; }
        [Column("duration")]
        public TimeSpan Duration { get; set; }
        [Column("cover_image")]
        public string? CoverImage { get; set; }
        [Column("audio_url")]
        public string? AudioUrl { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("public")]
        public bool Public { get; set; }

        // Navigation
        public ICollection<PlaylistTrack>? PlaylistTracks { get; set; }
    }
}
