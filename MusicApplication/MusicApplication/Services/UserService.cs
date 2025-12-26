using MusicApplication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
namespace MusicApplication.Services
{
    public class UserService
    {
        private readonly HttpClient httpClient;
        public UserService()
        {
            httpClient = ServiceHelper.GetService<HttpClient>();
        }

        public async Task<IEnumerable<User>> SearchUserAsync(string query)
        {
            try
            {
                var response = await httpClient.GetAsync($"api/User/search?query={Uri.EscapeDataString(query)}");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<User>>(data, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<User>();
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                    return new List<User>();
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request Exception: {ex.Message}");
                Console.WriteLine($"Inner Exception: {ex.InnerException?.Message}");
                return new List<User>();
            }
            catch (Exception ex)
            {
                // Handle exceptions (logging, rethrowing, etc.)
                Console.WriteLine($"Unexpected Error: {ex.Message}");
                return new List<User>();
            }
        }
    }
}
