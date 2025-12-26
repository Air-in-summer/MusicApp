using CommunityToolkit.Mvvm.ComponentModel;
using MusicApplication.Models;
using MusicApplication.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MusicApplication.ViewModels
{
    public enum SearchCategory
    {
        Songs,
        Artists
    }

    public class SearchCategoryType
    {
        public string Name { get; set; }
        public SearchCategory Type { get; set; }

        public static SearchCategoryType Songs => new() { Name = "Bài hát", Type = SearchCategory.Songs };
        public static SearchCategoryType Artists => new() { Name = "Nghệ sĩ", Type = SearchCategory.Artists };
    }

    public partial class SearchViewModel : ObservableObject
    {

        private readonly TrackService trackService;
        private readonly UserService userService;

        public ObservableCollection<Track> SearchResults { get; set; } = new ObservableCollection<Track>();
        public ObservableCollection<User> UserResults { get; set; } = new ObservableCollection<User>();

        [ObservableProperty]
        private ObservableCollection<object> currentResults = new();

        [ObservableProperty]
        private ObservableCollection<SearchCategoryType> categories = new()
        {
            SearchCategoryType.Songs,
            SearchCategoryType.Artists
        };

        [ObservableProperty]
        private SearchCategoryType selectedCategory = SearchCategoryType.Songs;


        public event PropertyChangedEventHandler PropertyChanged;
        // Constructor injection
        public SearchViewModel()
        {
            trackService = ServiceHelper.GetService<TrackService>();
            userService = ServiceHelper.GetService<UserService>();
        }

        public async Task SearchAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return;

            if (SelectedCategory.Type == SearchCategory.Songs /*"Bài hát"*/)
            {
                var results = SearchTracksAsync(keyword);
                CurrentResults = new ObservableCollection<object>(SearchResults.Cast<object>());
            }
            else if (SelectedCategory.Type == SearchCategory.Artists /*"Nghệ sĩ"*/)
            {
                var results = SearchUserAsync(keyword);
                CurrentResults = new ObservableCollection<object>(UserResults.Cast<object>());

            }
        }
        public async Task SearchTracksAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                SearchResults.Clear();
                return;
            }
            var responseTrack = await trackService.SearchTracksAsync(query);
            SearchResults.Clear();
            foreach (var item in responseTrack)
            {
                SearchResults.Add(item);
                Console.WriteLine($"Found song: {item.Title} - {item.Artist}");
            }
            OnPropertyChanged(nameof(SearchResults));
        }

        public async Task SearchUserAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                UserResults.Clear();
                return;
            }
            var responseUser = await userService.SearchUserAsync(query);
            UserResults.Clear();
            foreach (var item in responseUser)
            {
                UserResults.Add(item);
                Console.WriteLine($"Found song: {item.Username}");
            }
            OnPropertyChanged(nameof(UserResults));

        }
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value ? Colors.LightBlue : Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class SelectedCategoryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SearchCategoryType currentItem &&
                parameter is CollectionView collectionView &&
                collectionView.BindingContext is SearchViewModel vm)
            {
                return currentItem == vm.SelectedCategory ? Colors.LightBlue : Colors.LightGray;
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
    public class CategoryToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SearchCategoryType category && parameter is string paramStr)
            {
                return category.Type.ToString() == paramStr;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    public class HalfWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double totalWidth)
            {
                return (totalWidth - 30) / 2; // trừ khoảng cách (10 padding + spacing 10 x 2)
            }
            return 100;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
