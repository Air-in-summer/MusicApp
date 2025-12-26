using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using MusicApplication.Models;
using MusicApplication.Services;
using MusicApplication.ViewModels;

namespace MusicApplication.PersonalPages;

public partial class LibraryPage : ContentPage
{
    private FileResult selectedFile;
    //private readonly TrackService trackService;
    private readonly LibraryViewModel viewModel;
    private readonly PlayerService playerService;
    string durationString = string.Empty;
    public LibraryPage()
	{
		InitializeComponent();
        viewModel = new LibraryViewModel();
        BindingContext = viewModel;
        playerService = ServiceHelper.GetService<PlayerService>();
        //trackService = ServiceHelper.GetService<TrackService>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var username = SecureStorage.GetAsync("username").Result;
        ListTrackLabel.Text = "Danh sách các bài hát của " + username;
        LoadUserTracks();

        WeakReferenceMessenger.Default.Register<TrackDeletedMessage>(this, (r, m) =>
        {
            LoadUserTracks();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<TrackDeletedMessage>(this);
    }

    private async void LoadUserTracks()
    {
        var userId = SecureStorage.GetAsync("userID").Result;
        int userIdInt = int.Parse(userId);
        await viewModel.LoadUserTracks(userIdInt);
    }

    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        var customFileType = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "audio/mpeg", "audio/wav", "audio/mp3", "audio/x-wav", "audio/ogg" } }, // Android
                { DevicePlatform.iOS, new[] { "public.mp3", "public.audio" } }, // iOS
                { DevicePlatform.WinUI, new[] { ".mp3", ".wav", ".m4a" } }, // Windows
                { DevicePlatform.macOS, new[] { "public.audio" } } // macOS
            });

        var pickOptions = new PickOptions
        {
            PickerTitle = "Chọn tệp âm thanh",
            FileTypes = customFileType
        };

        selectedFile = await FilePicker.PickAsync(pickOptions);
        if (selectedFile != null)
        {
            var tempPath = Path.Combine(FileSystem.CacheDirectory, selectedFile.FileName);
            using (var sourceStream = await selectedFile.OpenReadAsync())
            using (var destinationStream = File.OpenWrite(tempPath))
            {
                await sourceStream.CopyToAsync(destinationStream);
            }
            var file = TagLib.File.Create(tempPath);
            var duration = file.Properties.Duration;
            durationString = duration.ToString(@"hh\:mm\:ss");
        }
    }

    private async void OnUploadClicked(object sender, EventArgs e)
    {
        if (selectedFile == null)
        {
            await DisplayAlert("Lỗi", "Vui lòng chọn một tệp trước khi upload.", "OK");
            return;
        }
        if (string.IsNullOrEmpty(TitleEntry.Text) || string.IsNullOrEmpty(GenreEntry.Text))
        {
            await DisplayAlert("Lỗi", "Vui lòng điền đầy đủ thông tin tiêu đề và nghệ sĩ.", "OK");
            return;
        }


        var content = new MultipartFormDataContent();
        var fileStream = await selectedFile.OpenReadAsync();

        content.Add(new StreamContent(fileStream), "File", selectedFile.FileName);
        content.Add(new StringContent(TitleEntry.Text), "Title");
        var userId = SecureStorage.GetAsync("userID").Result;
        content.Add(new StringContent(userId), "UserId");

        //lấy username làm artist 
        var username = SecureStorage.GetAsync("username").Result;
        content.Add(new StringContent(username), "Artist");

        content.Add(new StringContent(GenreEntry.Text), "Genre");
        content.Add(new StringContent(durationString), "Duration");

        Console.WriteLine(selectedFile.FileName);
        Console.WriteLine(TitleEntry.Text);
        Console.WriteLine(userId);
        Console.WriteLine(durationString);

        //upload
        bool upload = await viewModel.UploadTrackAsync(content);
        if (upload)
        {
            await DisplayAlert("Thông báo", "Tải lên thành công!", "OK");
            durationString = string.Empty;
        }
        else
        {
            await DisplayAlert("Thông báo", "Tải lên thất bại!", "OK");
            durationString = string.Empty;
        }

        // clear input 
        selectedFile = null;
        TitleEntry.Text = string.Empty;
        GenreEntry.Text = string.Empty;

        //load danh sách mới
        LoadUserTracks();
    }

    private async void MoreButton_Clicked(object sender, EventArgs e)
    {
        var imageButton = sender as ImageButton;
        var selectedTrack = imageButton?.CommandParameter as Track;
        if (selectedTrack == null) return;

        var bottomSheet = new BottomSheetContent(selectedTrack, true);
        this.ShowPopup(bottomSheet);
    }

    private async void TracksListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Track selectedTrack && !string.IsNullOrEmpty(selectedTrack.AudioUrl))
        {
            // Bỏ chọn để lần sau chọn lại cùng item vẫn được
            TracksListView.SelectedItem = null;

            try
            {
                //await playerService.PlayAsync(selectedTrack);
                var index = viewModel.Tracks.IndexOf(selectedTrack);
                var list = viewModel.Tracks.ToList(); // Convert ObservableCollection -> List

                //var playerService = ServiceHelper.GetService<PlayerService>();
                await playerService.PlayFromListAsync(list, index);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể phát nhạc: {ex.Message}", "OK");
            }
        }

    }
    private async void OnBackButtonTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}