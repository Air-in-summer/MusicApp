using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicApplication.Services
{
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

        public async Task<bool> IsApiReachable()
        {     
            try
            {
                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(3)
                };
                var response = await client.GetAsync("http://10.0.2.2:5296/api/Test/ping");
                Debug.WriteLine($"🌐 Ping server status: {(int)response.StatusCode} - {response.StatusCode}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Lỗi khi gọi API: {ex.Message}");
                return false;
            }
        }

        public void StopMonitoring()
        {
            _cts.Cancel();
        }
    }
}
