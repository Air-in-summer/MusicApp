using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using MusicApplication.Models;
using MusicApplication.PersonalPages;
using MusicApplication.Services;
using MusicApplication.ViewModels;
using System.Collections.ObjectModel;
namespace MusicApplication
{
    public partial class MainPage : ContentPage
    {
        private readonly MainViewModel viewModel;
        private readonly PlayerService playerService;

        public MainPage()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            playerService = ServiceHelper.GetService<PlayerService>();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var userName = SecureStorage.GetAsync("username").Result;
            UserNameLabel.Text = "Xin chào, " + userName;
            await viewModel.LoadSongs();
        }

        // sự kiện click vào 1 item của listview -> phát nhạc 
        private async void SongsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Track selectedTrack && !string.IsNullOrEmpty(selectedTrack.AudioUrl))
            {
                // Bỏ chọn để lần sau chọn lại cùng item vẫn được
                SongsListView.SelectedItem = null;

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

    }

}
