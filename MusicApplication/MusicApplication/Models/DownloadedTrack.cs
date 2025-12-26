using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace MusicApplication.Models
{
    public enum TrackStatus
    {
        Available = 0,
        Unavailable = 1
    }

    // Model của các track tải xuống
    // sử dụng SQLite 
    public class DownloadedTrack
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int TrackId { get; set; }
        public int UserId { get; set; } // để phân biệt người dùng (id người tải xuống)
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Genre { get; set; }
        public long DurationTicks { get; set; }
        public string LocalPath { get; set; } = string.Empty;
        public long DownloadedAtTicks { get; set; }
        public long ExpirationTimeTicks { get; set; } // thời hạn nghe offline

        public int TrackStatus { get; set; }

        // không lưu trong DB, chỉ dùng cho code
        [Ignore]
        public TimeSpan Duration => TimeSpan.FromTicks(DurationTicks);

        [Ignore]
        public DateTime DownloadedAt => new DateTime(DownloadedAtTicks, DateTimeKind.Utc);

        [Ignore]
        public DateTime ExpirationTime => new DateTime(ExpirationTimeTicks, DateTimeKind.Utc);
    }
}
