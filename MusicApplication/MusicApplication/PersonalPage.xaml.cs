using Microsoft.Maui.Storage;
using MusicApplication.Services;
using System.Net.WebSockets;
namespace MusicApplication
{
    public partial class PersonalPage : ContentPage
    {
        private readonly ConnectivityService connectivityService;
        public PersonalPage()
        {
            InitializeComponent();

            connectivityService = ServiceHelper.GetService<ConnectivityService>();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine(connectivityService.IsServerReachable);
            UpdateUIBasedOnConnectivity(connectivityService.IsServerReachable);           
            connectivityService.ConnectivityChanged += OnConnectivityChanged;
        } 

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            connectivityService.ConnectivityChanged -= OnConnectivityChanged;
        }

        private void OnConnectivityChanged(bool isReachable)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                UpdateUIBasedOnConnectivity(isReachable);
            });
        }

        private void UpdateUIBasedOnConnectivity(bool isOnline)
        {
            PlaylistsSection.IsVisible = isOnline;
            UploadsSection.IsVisible = isOnline;
        }

        private async void OnProfileTapped (object sender, EventArgs e)

        {
            // Chuyển đến trang ProfilePage
            await Navigation.PushAsync(new PersonalPages.ProfilePage());
        }

        private async void OnPlaylistsTapped (object sender, EventArgs e)
        {
            // Chuyển đến trang PlaylistsPage
            await Navigation.PushAsync(new PersonalPages.PlaylistPage());
        }

        private async void OnUploadsTapped(object sender, EventArgs e)
        {
            // Chuyển đến trang 
            await Navigation.PushAsync(new PersonalPages.LibraryPage());
        }


        private async void OnDownloadsTapped(object sender, EventArgs e)
        {
            // Chuyển đến trang DownloadPage
            await Navigation.PushAsync(new PersonalPages.DownloadPage());
        }

        
    }

}