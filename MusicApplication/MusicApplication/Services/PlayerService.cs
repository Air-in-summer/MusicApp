using CommunityToolkit.Maui.Views;
using CommunityToolkit.Maui.Core.Primitives;
using MusicApplication.Models;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;

namespace MusicApplication.Services
{
	// Cung cấp các dịch vụ phát nhạc nền tảng (background playback), quản lý trạng thái trình phát
	// và tương tác với thành phần MediaElement của UI.
	// Hỗ trợ cả luồng phát trực tuyến (Online) và phát từ bộ nhớ cục bộ (Offline).
    public class PlayerService : INotifyPropertyChanged
    {
        private MediaElement? player;
        public bool HasMediaElement => player != null;

        private Track? currentTrack;
	// Bài hát trực tuyến hiện tại đang được phát.
        public Track? CurrentTrack
        {
            get => currentTrack;
            private set
            {
                currentTrack = value;
                OnPropertyChanged(nameof(CurrentTrack));
                OnPropertyChanged(nameof(IsPlaying));
            }
        }

        private List<Track> currentTrackList = new();
        private int currentTrackIndex = -1;

        private DownloadedTrack? currentDownloadedTrack;
	// Bài hát ngoại tuyến hiện tại đang được phát.
        public DownloadedTrack? CurrentDownloadedTrack
        {
            get => currentDownloadedTrack;
            private set
            {
                currentDownloadedTrack = value;
                OnPropertyChanged(nameof(CurrentDownloadedTrack));
                OnPropertyChanged(nameof(IsPlaying));
            }
        }
        private List<DownloadedTrack> currentDownloadedList = new();
        private int currentDownloadedIndex = -1;

        public TimeSpan CurrentPosition => player?.Position ?? TimeSpan.Zero;
        public TimeSpan Duration => player?.Duration ?? TimeSpan.Zero;
        public bool IsPlaying => player?.CurrentState == MediaElementState.Playing;

        private bool isRepeat = false;
        public bool IsRepeat
        {
            get => isRepeat;
            set
            {
                isRepeat = value;
                if (player != null)
                {
                    player.ShouldLoopPlayback = value;
                }
                OnPropertyChanged(nameof(IsRepeat));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PlayerService()
        {
        }

        private bool _isDragging = false;

	// Liên kết đối tượng MediaElement từ UI Visual Tree vào Service.
	// <param name="me">Thực thể MediaElement được khởi tạo từ View.</param>
        public void AttachMediaElement(MediaElement me)
        {
            player = me;
            player.ShouldAutoPlay = false;
            player.ShouldLoopPlayback = IsRepeat;

            player.PositionChanged += (s, e) =>
            {
                // Ngừng cập nhật giao diện nếu người dùng đang thao tác kéo thanh Slider để tránh xung đột vị trí
                if (_isDragging) return;

                OnPropertyChanged(nameof(CurrentPosition));
                
                // Đồng bộ hóa trạng thái vị trí phát hiện tại vào bộ nhớ đệm mỗi 3 giây
                if (player.Position.TotalSeconds > 0 && (int)player.Position.TotalSeconds % 3 == 0)
                {
                    Preferences.Set("LastPosition", player.Position.TotalSeconds);
                }
            };

            player.StateChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(IsPlaying));
                if (e.NewState == MediaElementState.Playing || e.NewState == MediaElementState.Paused)
                {
                    OnPropertyChanged(nameof(Duration));
                }
            };

            player.MediaEnded += async (s, e) =>
            {
                await OnPlaybackEnded();
            };

            player.MediaOpened += (s, e) =>
            {
                // Đảm bảo Stream đã mở hoàn toàn trước khi thực hiện lệnh Seek để khôi phục vị trí cũ
                if (_resumePosition > TimeSpan.Zero)
                {
                    player.SeekTo(_resumePosition);
                    _resumePosition = TimeSpan.Zero; 
                }
            };

            LoadLastState();
        }

	// Bắt đầu phát nhạc từ một danh sách các bài hát trực tuyến.
        public async Task PlayFromListAsync(List<Track> trackList, int startIndex)
        {
            if (startIndex < 0 || startIndex >= trackList.Count) return;
            currentTrackList = trackList;
            currentTrackIndex = startIndex;
            await PlayAsync(currentTrackList[currentTrackIndex]);
        }

	// Bắt đầu phát nhạc từ một danh sách các bài hát ngoại tuyến (đã tải về).
        public async Task PlayFromDownloadedListAsync(List<DownloadedTrack> downloadedList, int startIndex)
        {
            if (startIndex < 0 || startIndex >= downloadedList.Count) return;
            currentDownloadedList = downloadedList;
            currentDownloadedIndex = startIndex;
            await PlayDownloadTrackAsync(currentDownloadedList[currentDownloadedIndex]);
        }

        public async Task PlayAsync(Track selectedTrack)
        {
            if (player == null) return;
            
            CurrentDownloadedTrack = null;
            CurrentTrack = selectedTrack;

            player.Source = MediaSource.FromUri(selectedTrack.AudioUrl);
            player.Play();

            SaveState();
        }

        public async Task PlayDownloadTrackAsync(DownloadedTrack downloadedTrack)
        {
            if (player == null) return;
            
            CurrentTrack = null;
            CurrentDownloadedTrack = downloadedTrack;

            player.Source = MediaSource.FromFile(downloadedTrack.LocalPath);
            player.Play();

            SaveState();
        }

        public async Task PlayNextAsync()
        {
            if (currentTrackList.Count > 0)
            {
                currentTrackIndex++;
                if (currentTrackIndex >= currentTrackList.Count) currentTrackIndex = 0;
                await PlayAsync(currentTrackList[currentTrackIndex]);
            }
            else if (currentDownloadedList.Count > 0)
            {
                currentDownloadedIndex++;
                if (currentDownloadedIndex >= currentDownloadedList.Count) currentDownloadedIndex = 0;
                await PlayDownloadTrackAsync(currentDownloadedList[currentDownloadedIndex]);
            }
        }

        public async Task PlayPreviousAsync()
        {
            if (currentTrackList.Count > 0)
            {
                currentTrackIndex--;
                if (currentTrackIndex < 0) currentTrackIndex = currentTrackList.Count - 1;
                await PlayAsync(currentTrackList[currentTrackIndex]);
            }
            else if (currentDownloadedList.Count > 0)
            {
                currentDownloadedIndex--;
                if (currentDownloadedIndex < 0) currentDownloadedIndex = currentDownloadedList.Count - 1;
                await PlayDownloadTrackAsync(currentDownloadedList[currentDownloadedIndex]);
            }
        }

        private async Task OnPlaybackEnded()
        {
            if (!IsRepeat)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await PlayNextAsync();
                });
            }
        }

        public void TogglePlayPause()
        {
            if (player == null) return;

            if (player.CurrentState == MediaElementState.Playing)
                player.Pause();
            else
                player.Play();
        }

        public void Seek(TimeSpan position)
        {
            player?.SeekTo(position);
            Preferences.Set("LastPosition", position.TotalSeconds);
            _isDragging = false; // Phục hồi cơ chế cập nhật UI sau khi thực hiện Seek
        }

        public void StartProgressTimer()
        {
            _isDragging = false;
        }

        public void StopProgressTimer()
        {
            _isDragging = true;
        }

        public void MusicDispose()
        {
            player?.Stop();
            CurrentTrack = null;
            CurrentDownloadedTrack = null;
            OnPropertyChanged(nameof(IsPlaying));
        }

        protected void OnPropertyChanged(string name)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }

	// Lưu trữ cấu hình bài hát hiện tại vào thiết lập của hệ điều hành.
	// Cho phép khôi phục phiên nghe nhạc sau khi ứng dụng bị tắt.
        private void SaveState()
        {
            if (CurrentTrack != null)
            {
                Preferences.Set("LastTrackType", "Online");
                Preferences.Set("LastTrack", JsonSerializer.Serialize(CurrentTrack));
            }
            else if (CurrentDownloadedTrack != null)
            {
                Preferences.Set("LastTrackType", "Offline");
                Preferences.Set("LastTrack", JsonSerializer.Serialize(CurrentDownloadedTrack));
            }
        }

        private TimeSpan _resumePosition = TimeSpan.Zero;

	// Truy xuất và tải lại trạng thái phiên nghe nhạc cuối cùng của người dùng.
        private void LoadLastState()
        {
            try
            {
                var type = Preferences.Get("LastTrackType", "");
                var json = Preferences.Get("LastTrack", "");
                var pos = Preferences.Get("LastPosition", 0.0);

                if (!string.IsNullOrEmpty(json) && player != null)
                {
                    // Lưu trữ tạm thời vị trí để khôi phục khi MediaElement kích hoạt sự kiện MediaOpened
                    _resumePosition = TimeSpan.FromSeconds(pos); 
                    if (type == "Online")
                    {
                        var track = JsonSerializer.Deserialize<Track>(json);
                        if (track != null)
                        {
                            CurrentTrack = track;
                            player.Source = MediaSource.FromUri(track.AudioUrl);
                        }
                    }
                    else if (type == "Offline")
                    {
                        var track = JsonSerializer.Deserialize<DownloadedTrack>(json);
                        if (track != null)
                        {
                            CurrentDownloadedTrack = track;
                            player.Source = MediaSource.FromFile(track.LocalPath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi LoadLastState: {ex.Message}");
            }
        }
    }
}
