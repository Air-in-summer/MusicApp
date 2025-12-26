using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace MusicAppBackend.Models
{
    [Table("playlists")]
    public class Playlist
    {
        [Key]
        [Column("playlist_id")]
        public int PlaylistId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User? User { get; set; }
        [Column("playlistname")]
        public string? PlaylistName { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<PlaylistTrack>? PlaylistTracks { get; set; }
    }
}
