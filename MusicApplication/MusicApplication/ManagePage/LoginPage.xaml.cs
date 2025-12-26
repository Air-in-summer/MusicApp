using MusicApplication.Services;

namespace MusicApplication.ManagePage;

public partial class LoginPage : ContentPage
{
	private readonly AuthService authService = ServiceHelper.GetService<AuthService>();
	public LoginPage()
	{
		InitializeComponent();
	}

	private async void OnLoginClicked(object sender, EventArgs e)
	{

		ErrorLabel.IsVisible = false;
		BusyIndicator.IsVisible = true;
		BusyIndicator.IsRunning = true;

		// nhận dữ liệu từ các trường nhập và gọi dịch vụ đăng nhập
		var email = EmailEntry.Text.Trim();
		var password = PasswordEntry.Text;
		var result = await authService.LoginAsync(email, password);


		BusyIndicator.IsVisible = false;
		BusyIndicator.IsRunning = false;
		if (!result.Success)
		{
			ErrorLabel.Text = result.ErrorMessage;
			ErrorLabel.IsVisible = true;
		}
		else
		{
			await DisplayAlert("Thành công", "Đăng nhập thành công!", "OK");
			// Chuyển sang AppShell (giao diện chính)
			Application.Current.MainPage = new AppShell();
		}
	}
}