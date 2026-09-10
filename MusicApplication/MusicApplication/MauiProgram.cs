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
                .UseMauiCommunityToolkitMediaElement() // Thêm MediaElement handler
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            // Đăng ký lớp chặn (Interceptor) để tự động gắn JWT Token
            builder.Services.AddTransient<AuthInterceptor>();

            // Khởi tạo HttpClient có nhét sẵn AuthInterceptor bên trong
            builder.Services.AddSingleton<HttpClient>(sp =>
            {
                var interceptor = sp.GetRequiredService<AuthInterceptor>();
                interceptor.InnerHandler = new HttpClientHandler();
                
                // Trả về HttpClient đã được "bọc" chức năng tự động gắn Token
                return new HttpClient(interceptor) { BaseAddress = new Uri("http://192.168.1.7:5296/") };
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
