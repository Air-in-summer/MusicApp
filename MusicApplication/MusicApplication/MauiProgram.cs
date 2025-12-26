using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MusicApplication.Services;
using MusicApplication.ViewModels;

namespace MusicApplication
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                return new HttpClient() { BaseAddress = new Uri("http://10.0.2.2:5296/") };
            });

            builder.Services.AddScoped<MusicApplication.Services.AuthService>();
            builder.Services.AddTransient<MusicApplication.Services.UserService>();
            builder.Services.AddTransient<MusicApplication.Services.TrackService>();
            builder.Services.AddTransient<MusicApplication.Services.PlaylistService>();
            builder.Services.AddScoped<MusicApplication.Services.DownloadService>();
            //builder.Services.AddScoped<MusicApplication.Services.ApiService>();
            builder.Services.AddSingleton<ConnectivityService>();
            builder.Services.AddSingleton<PlayerService>();
            builder.Services.AddSingleton<MiniPlayerViewModel>();



#if DEBUG
            builder.Logging.AddDebug();
#endif
        return builder.Build();
        }
    }
}
