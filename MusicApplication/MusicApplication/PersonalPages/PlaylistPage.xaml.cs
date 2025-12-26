using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using MusicApplication.Models;
using MusicApplication.ViewModels;
using System.Collections.ObjectModel;
using MusicApplication.Services;
using System.Threading.Tasks;


namespace MusicApplication.PersonalPages;

public partial class PlaylistPage : ContentPage
{
    private readonly PlaylistViewModel viewModel;
    public PlaylistPage()
    {
        InitializeComponent();
        viewModel = new PlaylistViewModel();
        BindingContext = viewModel;
        viewModel.IsSelectingPlaylists = false;
        
    }
    public PlaylistPage(bool isSelectingPlaylists, Track selectedTrack) : this()
    {
        viewModel.IsSelectingPlaylists = isSelectingPlaylists;
        //viewModel.IsNotSelectingPlaylists = !isSelectingPlaylists;
        viewModel.SelectedTrack = selectedTrack;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadPlaylists();

        WeakReferenceMessenger.Default.Register<PlaylistDeleteMessage>(this, (r, m) =>
        {
            LoadPlaylists();
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        WeakReferenceMessenger.Default.Unregister<PlaylistDeleteMessage>(this);
    }

    private async void LoadPlaylists()
    {
        await viewModel.LoadPlaylistsByIdAsync();
    }



    private async void OnCreatePlaylistClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(NewPlaylistEntry.Text))
        {
            await DisplayAlert("Lỗi", "Vui lòng nhập tên playlist.", "OK");
            return;
        }

        var playlistName = NewPlaylistEntry.Text;
        await viewModel.CreatePlaylistAsync(playlistName);
        NewPlaylistEntry.Text = string.Empty; // Clear the entry after creating
        LoadPlaylists(); // Reload the playlists to reflect the new one

    }

    private async void PlaylistsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Playlist selectedPlaylist)
        {
            // Navigate to the playlist details page
            await Navigation.PushAsync(new PlaylistTracksPage(selectedPlaylist.PlaylistId, selectedPlaylist.PlaylistName));
        }
    }

    /*private void PlaylistsCheckedListView_ItemSelected(object sender, SelectionItemChangedEventArgs e)
    {
        // Handle selection changes if needed
        // For example, you can update the UI or perform actions based on selected playlists
        if (e.SelectedItem is Playlist selectedPlaylist)
        {
            // Navigate to the playlist details page
            //await Navigation.PushAsync(new PlaylistTracksPage(selectedPlaylist.PlaylistId));
        }
    }*/


    private async void OnConfirmAddToPlaylistClicked(object sender, EventArgs e)
    {
        // lấy tất cả các playlist đã check trong CheckBox
        var selectedPlaylists = new ObservableCollection<Playlist>();
        foreach (var playlist in viewModel.Playlists.Where(p => p.IsSelected))
        {
            selectedPlaylists.Add(playlist);
        }

        foreach (var playlist in selectedPlaylists)
        {
            await viewModel.AddTrackToPlaylistAsync(playlist.PlaylistId, viewModel.SelectedTrack.TrackId);

        }
        await DisplayAlert("Thành công", "Đã thêm bài hát vào playlist đã chọn", "OK");
        await Navigation.PopAsync(); // Quay lại trang trước
    }

    private async void MoreButton_Clicked(object sender, EventArgs e)
    {
        var imageButton = sender as ImageButton;
        var selectedPlaylist = imageButton?.CommandParameter as Playlist;
        if (selectedPlaylist == null) return;

        var bottomSheet = new BottomSheetContent(selectedPlaylist.PlaylistId);
        this.ShowPopup(bottomSheet);
    }

    private async void OnBackButtonTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}