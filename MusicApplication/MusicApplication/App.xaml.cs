using MusicApplication.Services;
using System.Diagnostics;

namespace MusicApplication
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }
        private readonly ConnectivityService connectivityService;
        public App(IServiceProvider serviceProvider)
        {   
            InitializeComponent();
            Services = serviceProvider;
            connectivityService = ServiceHelper.GetService<ConnectivityService>();

            Task.Run(async () => await DownloadService.InitAsync());
            // Kiểm tra đăng nhập
            var token = SecureStorage.GetAsync("token").Result;
            if (string.IsNullOrEmpty(token))
            {
                MainPage = new NavigationPage(new ManagePage.HomePage());          
            }
            else
            {
                MainPage = new AppShell();
            }
        }
    }

    public static class ServiceHelper
    {
        public static T GetService<T>() where T : class
            => App.Services.GetService<T>();

        /*public static bool IsValidInput(string input)
        {
            // Chỉ cho phép chữ cái, số, dấu gạch dưới, dấu chấm, dấu @
            //var regex = new Regex("^[a-zA-Z0-9@._]+$");
            var regex = new Regex("^[a-zA-Z0-9@._\\s]+$");
            return regex.IsMatch(input);
        }*/
    }
}
