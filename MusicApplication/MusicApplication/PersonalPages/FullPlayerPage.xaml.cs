using CommunityToolkit.Maui.Views;
using MusicApplication.Models;
using MusicApplication.ViewModels;
using System.Diagnostics;

namespace MusicApplication.PersonalPages;

public partial class FullPlayerPage : ContentPage
{
    private bool isSeeking = false, isDragging = false;
    private readonly MiniPlayerViewModel playerViewModel;
    public FullPlayerPage()
    {
        InitializeComponent();
        playerViewModel = ServiceHelper.GetService<MiniPlayerViewModel>();
        BindingContext = playerViewModel;          
    }
    
    private void MoreButton_Clicked(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var parameter = button?.CommandParameter;
        
        if (button.CommandParameter is MiniPlayerViewModel)
        {
            if (playerViewModel.CurrentTrack != null)
            {
                var selectedTrack = playerViewModel.CurrentTrack;
                var popup = new BottomSheetContent(selectedTrack);
                this.ShowPopup(popup);
            }
            else if (playerViewModel.CurrentDownloadedTrack != null)
            {
                var selectedTrack = playerViewModel.CurrentDownloadedTrack;
                var popup = new BottomSheetContent(selectedTrack);
                this.ShowPopup(popup);
            }
            else return;
        }
        

    }
    // Khi user bắt đầu kéo
    private void ProgressSlider_DragStarted(object sender, EventArgs e)
    {
        isDragging = true;
        playerViewModel.PlayerService.StopProgressTimer();
        Debug.WriteLine("Drag Started");
    }

    // Khi user thả ra
    private void ProgressSlider_DragCompleted(object sender, EventArgs e)
    {
        isDragging = false;
        playerViewModel.SeekTo(TimeSpan.FromSeconds(ProgressSlider.Value));
        if (playerViewModel.IsPlaying)
        {
            playerViewModel.PlayerService.StartProgressTimer();
        }
        Debug.WriteLine("Drag Completed");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        isSeeking = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        isSeeking = false;
    }

    private async void OnBackButtonTapped(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}