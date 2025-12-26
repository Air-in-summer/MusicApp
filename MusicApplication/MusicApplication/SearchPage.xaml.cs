using CommunityToolkit.Maui.Views;
using MusicApplication.Models;
using MusicApplication.PersonalPages;
using MusicApplication.ViewModels;
using System.Globalization;
using MusicApplication.Services;
using System.Threading.Tasks;
namespace MusicApplication
{
    public partial class SearchPage : ContentPage
    {
        private readonly SearchViewModel _viewModel;
        private readonly PlayerService playerService;
        public SearchPage()
        {
            InitializeComponent();
            _viewModel = new SearchViewModel();
            playerService = ServiceHelper.GetService<PlayerService>();
            BindingContext = _viewModel;
        }

        private async void OnSearchBarPressed(object sender, EventArgs e)
        {
            string keyword = SearchBar.Text;
            await _viewModel.SearchAsync(keyword);
        }

        private async void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is SearchViewModel vm)
            {
                await vm.SearchAsync(e.NewTextValue);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing(); 
            SearchBar.Text = string.Empty;
        }
        private void OnFrameSizeChanged(object sender, EventArgs e)
        {
            if (sender is Frame frame && this.Width > 0)
            {
                // Chiều rộng màn hình chia đôi
                double itemWidth = this.Width / 2 ; // trừ margin/padding
                frame.WidthRequest = itemWidth;
            }
        }

        private async void OnCategoryChanged(object sender, EventArgs e)
        {
            // Gọi lại search nếu đã có từ khóa và người dùng đổi loại
            string keyword = SearchBar.Text;
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                await _viewModel.SearchAsync(keyword);
            }
        }
        private async void OnCategoryTapped(object sender, EventArgs e)
        {
            if (sender is Frame frame && frame.BindingContext is SearchCategoryType tappedCategory)
            {
                if (BindingContext is SearchViewModel vm && tappedCategory != vm.SelectedCategory)
                {
                    vm.SelectedCategory = tappedCategory;

                    string keyword = SearchBar.Text;
                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        await vm.SearchAsync(keyword);
                    }
                }
            }
        }


        private async void SearchResultsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Track selectedTrack && !string.IsNullOrEmpty(selectedTrack.AudioUrl))
            {
                // Bỏ chọn để lần sau chọn lại cùng item vẫn được
                SearchResultsListView.SelectedItem = null;

                try
                {
                    var index = _viewModel.SearchResults.IndexOf(selectedTrack);
                    var list = _viewModel.SearchResults.ToList(); // Convert ObservableCollection -> List
                    await playerService.PlayFromListAsync(list, index);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Lỗi", $"Không thể phát nhạc: {ex.Message}", "OK");
                }
            }

        }

        private async void SearchUserResultsListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if(e.SelectedItem is User user)
            {
                await Navigation.PushAsync(new ArtistPage(user));
            }
            SearchUserResultsListView.SelectedItem = null;
        }

        private void MoreButton_Clicked(object sender, EventArgs e)
        {
            var imageButton = sender as ImageButton;
            var selectedTrack = imageButton?.CommandParameter as Track;
            if (selectedTrack == null) return;

            var bottomSheet = new BottomSheetContent(selectedTrack);
            this.ShowPopup(bottomSheet);
        }


    }

    
}
