using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using MusicApplication.Models;
using MusicApplication.Services;
using System.Text.Json;

namespace MusicApplication.PersonalPages;

public partial class BottomSheetContent : Popup
{
	private readonly Track selectedTrack;
    private readonly DownloadedTrack downloadedTrack;
    private readonly int playlistId;
    private readonly TrackService trackService;
    private readonly DownloadService downloadService;
    public BottomSheetContent()
	{
		InitializeComponent();
        // Lấy kích thước thiết bị (đơn vị là pixel)
        var displayInfo = DeviceDisplay.MainDisplayInfo;

        // Đổi từ pixel sang đơn vị thiết kế
        var screenWidth = displayInfo.Width / displayInfo.Density;
        var screenHeight = displayInfo.Height / displayInfo.Density;

        // Chiều cao 2/3 màn hình
        var height = screenHeight * 2 / 3;

        this.Size = new Size(screenWidth, height);
        this.CanBeDismissedByTappingOutsideOfPopup = true;
        trackService = ServiceHelper.GetService<TrackService>();
    }

    // hàm tạo khi chọn More button của MainPage , SeachPage, ArtistPage
	public BottomSheetContent(Track _track) : this()
    {
        this.selectedTrack = _track;
        TitleLabel.Text = $"Tùy chọn cho: {selectedTrack.Title}";
        AddToPlaylistButton.IsVisible = true;
        DownloadTrackButton.IsVisible = true;
    }

    // hàm tạo khi chọn More button của PlaylistTracksPage
    public BottomSheetContent(Track _track, int _playlistId) : this()
    {
        this.selectedTrack = _track;
        this.playlistId = _playlistId;
        TitleLabel.Text = $"Tùy chọn cho: {selectedTrack.Title} trong Playlist ID: {playlistId}";
        AddToPlaylistButton.IsVisible = true;
        DeleteFromPlaylistButton.IsVisible = true;
        DownloadTrackButton.IsVisible = true;
    }

    // hàm tạo khi chọn more button của PlaylistPage
    public BottomSheetContent(int _playlistId) : this()
    {
        this.playlistId = _playlistId;
        TitleLabel.Text = $"Tùy chọn cho playlist có Playlist ID: {playlistId}";
        DeletePlaylistButton.IsVisible = true;
        
    }

    //hàm tạo khi chọn more button của LibraryPage
    public BottomSheetContent(Track _track, bool ok) : this()
    {
        this.selectedTrack = _track;
        TitleLabel.Text = $"Tùy chọn cho: {selectedTrack.Title}";
        AddToPlaylistButton.IsVisible = true;
        DeleteTrackButton.IsVisible = true;
        DownloadTrackButton.IsVisible = true;
    }
    //hàm tạo khi chọn more button của downloadpage
    public BottomSheetContent(DownloadedTrack _downloadedTrack) : this()
    {
        this.downloadedTrack = _downloadedTrack;
        downloadService = ServiceHelper.GetService<DownloadService>();
        TitleLabel.Text = $"Tùy chọn cho: {downloadedTrack.Title}";
        DeleteDownloadTrackButton.IsVisible = true;
    }

    private async void OnAddToPlaylistClicked(object sender, EventArgs e)
    {
        // TODO: Implement logic
        Close();

        var playlistPage = new PlaylistPage(true, selectedTrack);

        // Mở trang mới
        await Application.Current.MainPage.Navigation.PushAsync(playlistPage);
    }

    private async void OnDeleteFromPlaylistClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlert(
                            "Xác nhận",
                            "Bạn có chắc chắn muốn xóa bài hát khỏi playlist?",
                            "Có", // nút xác nhận
                            "Không" // nút hủy
        );

        if (!confirm)
        {
            Close();
            return;
        }
        try
        {
            var service = ServiceHelper.GetService<PlaylistService>();
            await service.DeleteTrackFromPlaylistAsync(playlistId, selectedTrack.TrackId);
            await Shell.Current.DisplayAlert("Thành công", "Đã xóa bài hát khỏi playlist", "OK");

            //MessagingCenter.Send(this, "TrackDeletedFromPlaylist");
            //WeakReferenceMessenger.Default.Send("TrackDeletedFromPlaylist");
            WeakReferenceMessenger.Default.Send(new TrackDeletedFromPlaylistMessage(selectedTrack.TrackId));

        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
        }
        Close();
    }

    private async void OnDeletePlaylistClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlert(
                            "Xác nhận",
                            "Bạn có chắc chắn muốn xóa playlist?",
                            "Có", // nút xác nhận
                            "Không" // nút hủy
        );

        if (!confirm)
        {
            Close();
            return;
        }
        try
        {
            var service = ServiceHelper.GetService<PlaylistService>();
            await service.DeletePlaylistAsync(playlistId);
            await Shell.Current.DisplayAlert("Thành công", "Đã xóa playlist", "OK");
            WeakReferenceMessenger.Default.Send(new PlaylistDeleteMessage(playlistId));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
        }
        Close();
    }

    private async void OnDeleteTrackClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlert(
                            "Xác nhận",
                            "Bạn có chắc chắn muốn xóa bài hát?",
                            "Có", // nút xác nhận
                            "Không" // nút hủy
        );

        if (!confirm)
        {
            Close();
            return;
        }
        try
        {
            var service = ServiceHelper.GetService<TrackService>();
            await service.DeleteTrackAsync(selectedTrack.TrackId);
            await Shell.Current.DisplayAlert("Thành công", "Đã xóa bài hát", "OK");
            WeakReferenceMessenger.Default.Send(new TrackDeletedMessage(playlistId));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", $"Không thể xóa: {ex.Message}", "OK");
        }
        Close();
    }

    private async void OnDownloadClicked(object sender, EventArgs e)
    {
        try
        {
            // track là bài hát được chọn (có URL và ID)
            var track = selectedTrack;

            // userId là ID người dùng hiện tại
            var userIdString = SecureStorage.GetAsync("userID").Result;
            int userId = int.Parse(userIdString);

            // Gọi API backend để kiểm tra track tồn tại           
            bool response = await trackService.GetTrackByTrackIdAsync(track.TrackId);
            if (!response)
            {
                await Shell.Current.DisplayAlert("Lỗi", "Bài hát không tồn tại hoặc đã bị xóa.", "OK");
                return;
            }

            // Kiểm tra track đã tải chưa 
            var service = ServiceHelper.GetService<DownloadService>();
            var downloaded = await service.GetDownloadedTrack(userId, selectedTrack.TrackId);
            if (downloaded)
            {
                await Shell.Current.DisplayAlert("Thông báo", "Bài hát đã được tải về thiết bị", "OK");
                return;
            }

            // Tải file nhạc từ S3
            string userFolderPath = Path.Combine(FileSystem.AppDataDirectory, userId.ToString());
            if (!Directory.Exists(userFolderPath))
            {
                Directory.CreateDirectory(userFolderPath);
            }
            //tạo thư mục của người dùng
            var fileName = $"track_{track.TrackId}_{userId}.mp3";            
            var filePath = await service.DownloadFileAsync(track.AudioUrl, userFolderPath, fileName);

            await service.AddDownloadedTrackAsync(track, filePath, userId);

            await Shell.Current.DisplayAlert("Thành công", "Đã tải bài hát về thiết bị", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Không thể tải bài hát", "OK");
        }
        Close();
    }

    private async void OnDeleteDownloadTrackClicked(object sender, EventArgs e)
    {
        bool confirm = await Shell.Current.DisplayAlert(
                            "Xác nhận",
                            "Bạn có chắc chắn muốn xóa bài hát đã tải xuống?",
                            "Có", // nút xác nhận
                            "Không" // nút hủy
        );

        if (!confirm)
        {
            Close();
            return;
        }
        try
        {
            await downloadService.DeleteDownloadedTrackAsync2(downloadedTrack.TrackId, downloadedTrack.UserId);
            WeakReferenceMessenger.Default.Send(new DownloadedTrackDeletedMessage(downloadedTrack.TrackId));
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Không thể xóa bài hát", "OK");
        }

        Close();
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Close();
    }
}