using CommunityToolkit.Maui.Views;
using MusicApplication.Services;

namespace MusicApplication.PersonalPages;

public partial class ProfilePage : ContentPage
{
    private readonly PlayerService playerService;
    public ProfilePage()
	{
		InitializeComponent();

        playerService = ServiceHelper.GetService<PlayerService>();
    }

    protected override void OnAppearing()
    {
        
        base.OnAppearing();
        var username = SecureStorage.GetAsync("username").Result;
        var email = SecureStorage.GetAsync("email").Result;
        LabelUserName.Text = username;
        LabelEmail.Text = email;
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        playerService.MusicDispose();
        SecureStorage.RemoveAll();
        // Chuyển về trang HomePage
        Application.Current.MainPage = new NavigationPage(new ManagePage.HomePage());
    }

    private void OnChangePassClicked(object sender, EventArgs e)
    {
        var popup = new ChangePasswordPopup();
        this.ShowPopup(popup);
    }

    private async void OnBackButtonTapped(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}