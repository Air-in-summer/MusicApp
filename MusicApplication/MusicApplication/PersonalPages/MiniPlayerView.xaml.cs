using MusicApplication.Services;
using MusicApplication.ViewModels;
using System.Diagnostics;
using System.Windows.Input;

namespace MusicApplication.PersonalPages;

public partial class MiniPlayerView : ContentView
{
    private readonly MiniPlayerViewModel viewModel;
    public MiniPlayerView()
    {
        InitializeComponent();
        viewModel = ServiceHelper.GetService<MiniPlayerViewModel>(); //new MiniPlayerViewModel();
        BindingContext = viewModel;
    }
    private async void OnTapped(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new FullPlayerPage());
    }
}