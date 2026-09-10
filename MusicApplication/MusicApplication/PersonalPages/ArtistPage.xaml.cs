using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using MusicApplication.Models;
using MusicApplication.Services;
using MusicApplication.ViewModels;

namespace MusicApplication.PersonalPages;

public partial class ArtistPage : ContentPage
{
    private User? user;
    private readonly LibraryViewModel viewModel;
    private readonly PlayerService playerService;

    public ArtistPage()
	{
		InitializeComponent();
        viewModel = new LibraryViewModel();
        BindingContext  = viewModel;
        playerService = ServiceHelper.GetService<PlayerService>();
    }

    public ArtistPage(User user) : this()
    {
        this.user = user;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        var userName = user.Username;
        LabelUserName.Text = userName;
        ListTrackLabel.Text = "Danh sách bài hát của " + userName;
        LoadUserTracks();       
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
    }

    private async void LoadUserTracks()
    {     
        await viewModel.LoadUserTracks(user.UserId);
    }

    private async void TracksListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        if (e.SelectedItem is Track selectedTrack && !string.IsNullOrEmpty(selectedTrack.AudioUrl))
        {
            // Bỏ chọn để lần sau chọn lại cùng item vẫn được
            TracksListView.SelectedItem = null;

            try
            {
                var index = viewModel.Tracks.IndexOf(selectedTrack);
                var list = viewModel.Tracks.ToList();

                await playerService.PlayFromListAsync(list, index);
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
        var selectedTrack = button?.CommandParameter as Track;

        if (selectedTrack == null) return;

        var popup = new BottomSheetContent(selectedTrack);
        this.ShowPopup(popup);
    }
    private async void OnBackButtonTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}