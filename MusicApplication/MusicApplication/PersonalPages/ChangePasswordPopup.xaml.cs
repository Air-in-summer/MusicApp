
using CommunityToolkit.Maui.Views;
using MusicApplication.Services;

namespace MusicApplication.PersonalPages;

public partial class ChangePasswordPopup : Popup
{
    private readonly AuthService authService;
    public ChangePasswordPopup()
    {
        InitializeComponent();
        authService = ServiceHelper.GetService<AuthService>();
        // Lấy kích thước thiết bị (đơn vị là pixel)
        var displayInfo = DeviceDisplay.MainDisplayInfo;

        // Đổi từ pixel sang đơn vị thiết kế
        var screenWidth = displayInfo.Width / displayInfo.Density;
        var screenHeight = displayInfo.Height / displayInfo.Density;

        // Chiều cao 2/3 màn hình
        var height = screenHeight * 2 / 3;

        this.Size = new Size(screenWidth, height);
        this.CanBeDismissedByTappingOutsideOfPopup = true;
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        string oldPassword = OldPasswordEntry.Text;
        string newPassword = NewPasswordEntry.Text;
        string confirmPassword = ConfirmPasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(oldPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            await Shell.Current.DisplayAlert("Lỗi", "Vui lòng điền đầy đủ thông tin", "OK");
            return;
        }

        if (newPassword != confirmPassword)
        {
            await Shell.Current.DisplayAlert("Lỗi", "Mật khẩu xác nhận không khớp", "OK");
            return;
        }

        var result = await authService.ChangePassword(oldPassword, newPassword);

        // TODO: Gọi API hoặc xử lý logic đổi mật khẩu
        if (result.Success)
        {
            await Shell.Current.DisplayAlert("Thành công", "Mật khẩu đã được thay đổi", "OK");
        }
        else
        {
            await Shell.Current.DisplayAlert("Thất bại ", result.ErrorMessage, "OK");
        }
        // Đóng popup
        Close();
    }

    private async void OnDisposeClicked(object sender, EventArgs e)
    {
        Close();
    }
}