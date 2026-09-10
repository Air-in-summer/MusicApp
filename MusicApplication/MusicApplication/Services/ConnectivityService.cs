using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApplication.Services
{
    // Lớp quản lý và giám sát trạng thái kết nối mạng của ứng dụng tới máy chủ.
    public class ConnectivityService
    {
        private bool isServerReachable;
        public bool IsServerReachable => isServerReachable;
        private readonly HttpClient httpClient;
        public event Action<bool> ConnectivityChanged;
        private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5); // kiểm tra mỗi 5s
        private CancellationTokenSource _cts = new();
        public ConnectivityService()
        {
            httpClient = ServiceHelper.GetService<HttpClient>();
            StartMonitoring();
        }

        // Bắt đầu tiến trình nền để liên tục giám sát trạng thái kết nối đến máy chủ theo chu kỳ.
        private void StartMonitoring()
        {
            _ = Task.Run(async () =>
            {               

                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        bool currentStatus = await IsApiReachable();

                        if (currentStatus != isServerReachable)
                        {
                            isServerReachable = currentStatus;
                            Debug.WriteLine($"✅ Trạng thái thay đổi: {(isServerReachable ? "Online" : "Offline")}");
                            ConnectivityChanged?.Invoke(isServerReachable);
                        }
                        else
                        {
                            Debug.WriteLine($"✅ Trạng thái không đổi: {(isServerReachable ? "Online" : "Offline")}");
                        }

                        await Task.Delay(_checkInterval, _cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        Debug.WriteLine("🛑 Dừng kiểm tra kết nối.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"⚠️ Lỗi trong StartMonitoring: {ex.Message}");
                        await Task.Delay(_checkInterval);
                    }
                }
            });
        }

        // Thực hiện yêu cầu HTTP GET để kiểm tra tính khả dụng của API máy chủ.
        public async Task<bool> IsApiReachable()
        {     
            try
            {
                // Tạo một CancellationToken tự động hủy (cancel) sau đúng 3 giây
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));

                // Truyền cts.Token vào hàm GetAsync của httpClient dùng chung
                var response = await httpClient.GetAsync("api/Test/ping", cts.Token);
                
                Debug.WriteLine($"🌐 Ping server status: {(int)response.StatusCode} - {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Lỗi khi gọi API: {ex.Message}");
                return false;
            }
        }

        // Hủy bỏ luồng thực thi và dừng tiến trình giám sát kết nối.
        public void StopMonitoring()
        {
            _cts.Cancel();
        }
    }
}
