using MusicApplication.Models;
using MusicApplication.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MusicApplication.ViewModels
{
    public class MiniPlayerViewModel : INotifyPropertyChanged
    {
        public PlayerService PlayerService { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public Track? CurrentTrack => PlayerService.CurrentTrack;
        public DownloadedTrack? CurrentDownloadedTrack => PlayerService.CurrentDownloadedTrack;

        //kiểm tra play/pause
        public bool IsPlaying => PlayerService.IsPlaying;
        public ICommand TogglePlayPauseCommand { get; }
        public string PlayPauseIcon => IsPlaying ? "pause_icon.png" : "play_icon.png"; // path tới icon


        // vị trí hiện tại và duration
        public TimeSpan CurrentPosition => PlayerService.CurrentPosition;
        public TimeSpan Duration => PlayerService.Duration;

        // 🟢 Sử dụng để binding cho Slider an toàn
        public double CurrentSeconds => CurrentPosition.TotalSeconds;
        public double DurationSeconds => Duration.TotalSeconds;

        public double ProgressValue => Duration.TotalSeconds > 0
                                        ? CurrentPosition.TotalSeconds / Duration.TotalSeconds
                                        : 0;

        public string ProgressText => $"{CurrentPosition:hh\\:mm\\:ss} / {Duration:hh:\\mm\\:ss}";

        public string DisplayTitle => CurrentTrack?.Title ?? CurrentDownloadedTrack?.Title ?? "Không có bài nào đang phát";
        public string DisplayArtist => CurrentTrack?.Artist ?? CurrentDownloadedTrack?.Artist ?? "Nghệ sĩ không rõ";

        //chuyển next/prev
        public ICommand PlayNextCommand { get; }
        public ICommand PlayPreviousCommand { get; }

        //lặp lại 
        public bool IsRepeat
        {
            get => PlayerService.IsRepeat;
            set
            {
                PlayerService.IsRepeat = value;
                OnPropertyChanged(nameof(IsRepeat));
                OnPropertyChanged(nameof(RepeatIcon));
            }
        }
        public ICommand ToggleRepeatCommand { get; }

        public string RepeatIcon => IsRepeat ? "repeat_icon.png" : "no_repeat_icon.png";

        public MiniPlayerViewModel()
        {
            Debug.WriteLine("✅ MiniPlayerViewModel được tạo");
            PlayerService = ServiceHelper.GetService<PlayerService>();

            TogglePlayPauseCommand = new Command(() =>
            {
                PlayerService.TogglePlayPause();
                OnPropertyChanged(nameof(PlayPauseIcon));
            });

            

            PlayerService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(PlayerService.CurrentTrack) ||
                    e.PropertyName == nameof(PlayerService.CurrentDownloadedTrack) ||
                    e.PropertyName == nameof(PlayerService.IsPlaying) ||
                    e.PropertyName == nameof(PlayerService.CurrentPosition))
                {
                    OnPropertyChanged(nameof(CurrentTrack));
                    OnPropertyChanged(nameof(CurrentDownloadedTrack));

                    OnPropertyChanged(nameof(IsPlaying));
                    OnPropertyChanged(nameof(PlayPauseIcon));

                    OnPropertyChanged(nameof(CurrentSeconds));
                    OnPropertyChanged(nameof(DurationSeconds));

                    OnPropertyChanged(nameof(CurrentPosition));
                    OnPropertyChanged(nameof(Duration));
                    OnPropertyChanged(nameof(ProgressValue));
                    OnPropertyChanged(nameof(ProgressText));

                    OnPropertyChanged(nameof(DisplayTitle));
                    OnPropertyChanged(nameof(DisplayArtist));
                }
            };

            PlayNextCommand = new Command(async () =>
            {
                await PlayerService.PlayNextAsync();
            });

            PlayPreviousCommand = new Command(async () =>
            {
                await PlayerService.PlayPreviousAsync();
            });

            ToggleRepeatCommand = new Command(() =>
            {
                IsRepeat = !IsRepeat;
            });
        }
        public void SeekTo(TimeSpan position)
        {

            try
            {
                PlayerService.Seek(position);
                Debug.WriteLine("Seek ok");
            }
            catch(Exception ex) 
            {
                Debug.WriteLine($"Failed to seek to {ex.Message}");
            }
        }

        protected void OnPropertyChanged(string name)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}
