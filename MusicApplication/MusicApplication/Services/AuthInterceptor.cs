using System.Net;
using System.Net.Http.Headers;

namespace MusicApplication.Services
{
    // Lớp chặn (Interceptor) đứng giữa HttpClient và Server
    // Nhiệm vụ: Tự động đính kèm Access Token vào mọi Request, và tự động Refresh Token khi gặp lỗi 401
    public class AuthInterceptor : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Lấy Access Token hiện tại từ SecureStorage
                var token = await SecureStorage.GetAsync("token");
                if (!string.IsNullOrEmpty(token))
                {
                    // Gắn Token vào Header Authorization của Request
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // 2. Chuyển tiếp Request tới Backend
                var response = await base.SendAsync(request, cancellationToken);

                // Ghi log nếu Response không thành công (Status Code >= 400)
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[AuthInterceptor Error] Request URL: {request.RequestUri} | Status: {(int)response.StatusCode} ({response.ReasonPhrase})");
                }

                // 3. Xử lý trường hợp Access Token hết hạn (Lỗi 401 Unauthorized)
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine($"[AuthInterceptor Warning] Lỗi 401 Unauthorized tại {request.RequestUri}. Đang tiến hành làm mới Token...");

                    // Gọi AuthService thông qua ServiceHelper để tránh lỗi Circular Dependency
                    var authService = ServiceHelper.GetService<AuthService>();
                    if (authService != null)
                    {
                        // Thực hiện yêu cầu cấp lại Token mới
                        bool isRefreshed = await authService.RefreshTokenAsync();
                        
                        if (isRefreshed)
                        {
                            Console.WriteLine("[AuthInterceptor Success] Làm mới Token thành công. Đang gửi lại Request ban đầu...");

                            // Lấy Access Token mới vừa được cập nhật
                            var newToken = await SecureStorage.GetAsync("token");
                            
                            // Cần phải sao chép (Clone) Request cũ vì .NET không cho phép gửi lại cùng một đối tượng Request 2 lần
                            var clonedRequest = await CloneRequest(request);
                            clonedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                            
                            // Gửi lại Request lần 2 với Token mới
                            response = await base.SendAsync(clonedRequest, cancellationToken);

                            if (!response.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"[AuthInterceptor Error] Request sau khi gửi lại thất bại | Status: {(int)response.StatusCode} ({response.ReasonPhrase})");
                            }
                        }
                        else
                        {
                            Console.WriteLine("[AuthInterceptor Error] Làm mới Token thất bại hoặc Refresh Token đã hết hạn. Đang chuyển hướng về màn hình đăng nhập...");

                            // Xóa dữ liệu phiên đăng nhập
                            SecureStorage.RemoveAll();

                            // Điều hướng người dùng về màn hình đăng nhập (yêu cầu thực thi trên luồng UI chính)
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Application.Current.MainPage = new NavigationPage(new ManagePage.HomePage());
                            });
                        }
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AuthInterceptor Exception] Lỗi ngoại lệ trong quá trình xử lý HTTP Request: {ex.Message}");
                throw;
            }
        }

        // Hàm hỗ trợ nhân bản Request
        private async Task<HttpRequestMessage> CloneRequest(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version
            };

            // Copy Content (Body)
            if (request.Content != null)
            {
                var ms = new MemoryStream();
                await request.Content.CopyToAsync(ms);
                ms.Position = 0;
                clone.Content = new StreamContent(ms);

                foreach (var header in request.Content.Headers)
                {
                    clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            // Copy Headers (bỏ qua Authorization để tránh trùng lặp khi retry với token mới)
            foreach (var header in request.Headers)
            {
                if (header.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                    continue;
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Copy Options
            foreach (var option in request.Options)
            {
                clone.Options.Set(new HttpRequestOptionsKey<object?>(option.Key), option.Value);
            }

            return clone;
        }
    }
}
