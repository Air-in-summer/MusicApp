using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace MusicAppBackend.Models
{   
    [Table("users")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("username")]
        public string? Username { get; set; }
        [Column("email")]
        public string? Email { get; set; }
        [Column("password_hash")]
        public string? PasswordHash { get; set; }
        [Column("password_salt")]
        public string? PasswordSalt { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("role")]
        public string? Role { get; set; }
        [Column("cover_image")]
        public string? CoverImage { get; set; }

        // Navigation
        public ICollection<Track>? Tracks { get; set; }
        public ICollection<Playlist>? Playlists { get; set; }
    }
}
