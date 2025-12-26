using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using MusicApplication.Models;
using MusicApplication.ViewModels;
using MusicApplication.Services;
namespace MusicApplication.PersonalPages;

public partial class PlaylistTracksPage : ContentPage
{
	private readonly PlaylistTrackViewModel viewModel;
    private readonly PlayerService playerService;
    int playlistId;
    string playlistName;
    public PlaylistTracksPage()
	{
		InitializeComponent();
		viewModel = new PlaylistTrackViewModel(); // Default constructor, will be set later
        playerService = ServiceHelper.GetService<PlayerService>();
        BindingContext = viewModel;
    }
    public PlaylistTracksPage(int _playlistId, string _playlistName)
	{
		InitializeComponent();
		viewModel = new PlaylistTrackViewModel();
		BindingContext = viewModel;
        this.playlistId = _playlistId;
        this.playlistName = _playlistName;
    }

	protected override async void OnAppearing()
	{
		base.OnAppearing();
        LabelPlaylistName.Text = this.playlistName;
        LoadTracks();
        
        WeakReferenceMessenger.Default.Register<TrackDeletedFromPlaylistMessage>(this, (r, m) =>
        {
            LoadTracks();
        }); 
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        //MessagingCenter.Unsubscribe<BottomSheetContent>(this, "TrackDeletedFromPlaylist");
        WeakReferenceMessenger.Default.Unregister<TrackDeletedFromPlaylistMessage>(this);
    }
    private async void LoadTracks()
    {
        await viewModel.LoadTracksByPlaylistIdAsync(playlistId);
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
    private async void MoreButton_Clicked(object sender, EventArgs e)
    {
        var imageButton = sender as ImageButton;
        var selectedTrack = imageButton?.CommandParameter as Track;
        if (selectedTrack == null) return;

        var bottomSheet = new BottomSheetContent(selectedTrack, playlistId);
        this.ShowPopup(bottomSheet);
    }

    private async void OnBackButtonTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}