using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MusicAppBackend.Models
{
    [Table("playlist_tracks")]
#if NET8_0_OR_GREATER
    [PrimaryKey(nameof(PlaylistId), nameof(TrackId))]
#endif
    public class PlaylistTrack
    {
        [Column("playlist_id")]
        public int PlaylistId { get; set; }
        [ForeignKey("PlaylistId")]
        public Playlist? Playlist { get; set; }
        [Column("track_id")]
        public int TrackId { get; set; }
        [ForeignKey("TrackId")]
        public Track? Track { get; set; }
        [Column("added_at")]
        public DateTime AddedAt { get; set; }
    }
}
