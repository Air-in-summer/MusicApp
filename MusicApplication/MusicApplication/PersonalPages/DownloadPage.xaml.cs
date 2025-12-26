using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using MusicApplication.Models;
using MusicApplication.ViewModels;
using MusicApplication.Services;


namespace MusicApplication.PersonalPages;

public partial class DownloadPage : ContentPage
{
	private readonly DownloadViewModel viewModel;
    private readonly PlayerService playerService;
    public DownloadPage()
	{
		InitializeComponent();
		viewModel = new DownloadViewModel();
        playerService = ServiceHelper.GetService<PlayerService>();
        BindingContext = viewModel;
	}

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        LoadDownloadedTracks();
        WeakReferenceMessenger.Default.Register<DownloadedTrackDeletedMessage>(this, (r, m) =>
        {
            LoadDownloadedTracks();
        });
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<DownloadedTrackDeletedMessage>(this);

    }

    private async void LoadDownloadedTracks()
    {
        await viewModel.LoadDownloadedTracks();
    }

    private async void DownloadListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is DownloadedTrack selectedTrack && !string.IsNullOrEmpty(selectedTrack.LocalPath))
        {
            // Bỏ chọn để lần sau chọn lại cùng item vẫn được
            DownloadListView.SelectedItem = null;

            try
            {
                //await playerService.PlayDownloadTrackAsync(selectedTrack);
                var index = viewModel.DownloadedTracks.IndexOf(selectedTrack);
                var list = viewModel.DownloadedTracks.ToList(); // Convert ObservableCollection -> List

                //var playerService = ServiceHelper.GetService<PlayerService>();
                await playerService.PlayFromDownloadedListAsync(list, index);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Lỗi", $"Không thể phát nhạc: {ex.Message}", "OK");
            }
        }
    }

    private void MoreButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var selectedTrack = button?.CommandParameter as DownloadedTrack;

        if (selectedTrack == null) return;

        var popup = new BottomSheetContent(selectedTrack);
        this.ShowPopup(popup);
    }

    /*private async void DownloadListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selected = e.CurrentSelection.FirstOrDefault() as DownloadedTrackViewModel;
        if (selected == null) return;

        if (selected.Status == "Có thể phát")
        {            var player = AudioManager.Current;
            await player.StopAsync();
            await player.PlayAsync(selected.LocalPath);
        }
        else
        {
            await DisplayAlert("Thông báo", "Bài hát đã hết hạn. Cần kết nối mạng để tiếp tục nghe.", "OK");
        }

        DownloadListView.SelectedItem = null;
    }*/

    private async void OnBackButtonTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}