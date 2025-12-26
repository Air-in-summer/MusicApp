using MusicApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Plugin.Maui.Audio;
using System.ComponentModel;
using System.Diagnostics;

namespace MusicApplication.Services
{
    public class PlayerService : INotifyPropertyChanged
    {
        private readonly IAudioManager audioManager;
        private IAudioPlayer? player;
        private readonly HttpClient httpClient;
        private bool isTimerRunning = false;
        private Track? currentTrack;
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

        //tiến độ 
        private TimeSpan currentPosition;
        public TimeSpan CurrentPosition
        {
            get => currentPosition;
            private set
            {
                currentPosition = value;
                OnPropertyChanged(nameof(CurrentPosition));
            }
        }

        //lặp hay không 
        private bool isRepeat = false;
        public bool IsRepeat
        {
            get => isRepeat;
            set
            {
                isRepeat = value;
                OnPropertyChanged(nameof(IsRepeat));
            }
        }

        public TimeSpan Duration => TimeSpan.FromSeconds(player?.Duration ?? 0);
        public bool IsPlaying => player?.IsPlaying ?? false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public PlayerService()
        {
            audioManager = AudioManager.Current;
            httpClient = new HttpClient();
        }

        public async Task PlayFromListAsync(List<Track> trackList, int startIndex)
        {
            if (startIndex < 0 || startIndex >= trackList.Count)
                return;

            currentTrackList = trackList;
            currentTrackIndex = startIndex;
            await PlayAsync(currentTrackList[currentTrackIndex]);
        }

        public async Task PlayFromDownloadedListAsync(List<DownloadedTrack> downloadedList, int startIndex)
        {
            if (startIndex < 0 || startIndex >= downloadedList.Count)
                return;

            currentDownloadedList = downloadedList;
            currentDownloadedIndex = startIndex;
            await PlayDownloadTrackAsync(currentDownloadedList[currentDownloadedIndex]);
        }

        public async Task PlayAsync(Track selectedTrack)
        {
            try
            {
                CurrentDownloadedTrack = null;
                currentDownloadedList.Clear();
                if (player != null)
                {
                    player.PlaybackEnded -= OnPlaybackEnded;
                    player.Stop();
                    player.Dispose();
                }

                var response = await httpClient.GetAsync(selectedTrack.AudioUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync();
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                player = audioManager.CreatePlayer(memoryStream);

                CurrentTrack = selectedTrack;
                player.Play();
                StartProgressTimer();

                player.PlaybackEnded += OnPlaybackEnded;

                OnPropertyChanged(nameof(IsPlaying));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi phát nhạc: {ex.Message}");
                throw;
            }
        }

        /////////////////////////////    
        public async Task PlayDownloadTrackAsync(DownloadedTrack downloadedTrack)
        {
            try
            {
                CurrentTrack = null;
                currentTrackList.Clear();
                if (player != null)
                {
                    player.PlaybackEnded -= OnPlaybackEnded;
                    player.Stop();
                    player.Dispose();
                }

                var localPath = downloadedTrack.LocalPath;

                if (!File.Exists(localPath))
                    throw new FileNotFoundException("Không tìm thấy file nhạc", localPath);

                using var fileStream = File.OpenRead(localPath);
                var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                player = audioManager.CreatePlayer(memoryStream);
                CurrentDownloadedTrack = downloadedTrack;
                player.Play();
                StartProgressTimer();

                player.PlaybackEnded += OnPlaybackEnded;

                OnPropertyChanged(nameof(IsPlaying));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi phát nhạc offline: {ex.Message}");
                throw;
            }
        }

        public async Task PlayNextAsync()
        {
            if (/*CurrentTrack != null &&*/ currentTrackList.Count > 0)
            {
                currentTrackIndex++;
                if (currentTrackIndex >= currentTrackList.Count)
                    currentTrackIndex = 0; // quay vòng hoặc bạn có thể bỏ if

                await PlayAsync(currentTrackList[currentTrackIndex]);
            }
            else if (/*CurrentDownloadedTrack != null &&*/ currentDownloadedList.Count > 0)
            {
                currentDownloadedIndex++;
                if (currentDownloadedIndex >= currentDownloadedList.Count)
                    currentDownloadedIndex = 0;

                await PlayDownloadTrackAsync(currentDownloadedList[currentDownloadedIndex]);
            }
        }

        public async Task PlayPreviousAsync()
        {
            if (CurrentTrack != null && currentTrackList.Count > 0)
            {
                currentTrackIndex--;
                if (currentTrackIndex < 0)
                    currentTrackIndex = currentTrackList.Count - 1;

                await PlayAsync(currentTrackList[currentTrackIndex]);
            }
            else if (CurrentDownloadedTrack != null && currentDownloadedList.Count > 0)
            {
                currentDownloadedIndex--;
                if (currentDownloadedIndex < 0)
                    currentDownloadedIndex = currentDownloadedList.Count - 1;

                await PlayDownloadTrackAsync(currentDownloadedList[currentDownloadedIndex]);
            }
        }

        private async void OnPlaybackEnded(object? sender, EventArgs e)
        {
            
            DownloadedTrack cloneDownloadedTrack = CurrentDownloadedTrack;
            Track cloneTrack = CurrentTrack;
            CurrentDownloadedTrack = null;
            CurrentTrack = null;
            OnPropertyChanged(nameof(IsPlaying));

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    //await PlayNextAsync();
                    if (IsRepeat)
                    {
                        // 🔁 Phát lại bài hiện tại
                        if (cloneTrack != null)
                            await PlayAsync(cloneTrack);
                        else if (cloneDownloadedTrack != null)
                            await PlayDownloadTrackAsync(cloneDownloadedTrack);
                    }
                    else
                    {
                        //await Task.Delay(100); // Đảm bảo đã giải phóng player
                        await PlayNextAsync(); // ▶ Phát bài tiếp theo nếu không lặp
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi xử lý PlaybackEnded: {ex.Message}");
                }
            });
        }
        public void TogglePlayPause()
        {
            if (player == null) return;

            if (player.IsPlaying)
                player.Pause();
            else
            {
                player.Play();
                StartProgressTimer();
            }

            OnPropertyChanged(nameof(IsPlaying));
        }

        public void MusicDispose()
        {
            if (player != null)
            {
                player.Stop();
                player.Dispose();
                player = null;
            }

            CurrentTrack = null;
            CurrentDownloadedTrack = null;
            StopProgressTimer();
            OnPropertyChanged(nameof(IsPlaying));
        }

        public void StartProgressTimer()
        {
            if (isTimerRunning) return;

            isTimerRunning = true;
            Application.Current.Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
            {
                if (player != null && player.IsPlaying)
                {
                    CurrentPosition = TimeSpan.FromSeconds(player.CurrentPosition);
                    return true; // tiếp tục timer
                }
                isTimerRunning = false;
                return false; // dừng timer
            });
        }
        public void Seek(TimeSpan position)
        {
            if (player == null) return;
            /*Console.WriteLine($"🔁 Seek to: {position}");
            double seconds = position.TotalSeconds;
            player.Seek(seconds);
            CurrentPosition = position;*/
            double seconds = position.TotalSeconds;
            double duration = player.Duration;

            if (seconds >= duration)
            {
                seconds = duration - 0.1;
                if (seconds < 0) seconds = 0;
            }

            Debug.WriteLine($"🔁 Seek to: {TimeSpan.FromSeconds(seconds)}");

            try
            {
                player.Seek(seconds);
                CurrentPosition = TimeSpan.FromSeconds(seconds);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Seek failed: {ex.Message}");
            }
        }

        public void StopProgressTimer()
        {
            isTimerRunning = false;
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