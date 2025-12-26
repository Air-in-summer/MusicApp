using MusicApplication.Services;

namespace MusicApplication.ManagePage;

public partial class RegisterPage : ContentPage
{

	private readonly AuthService authService = ServiceHelper.GetService<AuthService>();

	public RegisterPage()
	{
		InitializeComponent();
	}

	private async void OnRegisterClicked(object sender, EventArgs e)
	{
		//ErrorLabel.IsVisible = false;
		BusyIndicator.IsVisible = true;
		BusyIndicator.IsRunning = true;

		// nhận dữ liệu từ các trường nhập
		var username = UsernameEntry.Text.Trim();
		var email = EmailEntry.Text.Trim();
		var password = PasswordEntry.Text;
		var confirmPassword = ConfirmPasswordEntry.Text;
		
		
		var result = await authService.RegisterAsync(username, email, password, confirmPassword);

		BusyIndicator.IsVisible = false;
		BusyIndicator.IsRunning = false;
		if (!result.Success)
		{
			ErrorLabel.Text = result.ErrorMessage;
			ErrorLabel.IsVisible = true;
		}
		else
		{
			await DisplayAlert("Thành công", "Đăng ký thành công!", "OK");
			//trở về trang homepage
			await Navigation.PopToRootAsync();
		}
	}
}