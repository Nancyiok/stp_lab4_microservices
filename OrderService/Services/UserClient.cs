using System.Net;
using OrderService.Models;

namespace OrderService.Services
{
    public class UserClient
    {
        private readonly HttpClient _httpClient;

        public UserClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<User?> GetUserAsync(int userId)
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<User>();
        }
    }
}
