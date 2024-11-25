using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace BlazorWASM.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string> GetTokenAsync(); // Ændret til async
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private const string TOKEN_KEY = "authToken";
        private const string REFRESH_TOKEN_KEY = "refreshToken";

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            try 
            {
                var loginRequest = new
                {
                    Email = email,  
                    Password = password
                };

                // Log den fulde URL
                Console.WriteLine($"Making request to: {_httpClient.BaseAddress}api/Users/login");
                
                // Log request data
                Console.WriteLine($"Request data: Email={email}");

                var response = await _httpClient.PostAsJsonAsync("api/Users/login", loginRequest);
                
                // Log response detaljer
                Console.WriteLine($"Response Status: {response.StatusCode}");
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response Content: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                    await _localStorage.SetItemAsync(TOKEN_KEY, result.AccessToken);
                    await _localStorage.SetItemAsync(REFRESH_TOKEN_KEY, result.RefreshToken);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                // Log eventuelle exceptions
                Console.WriteLine($"Exception occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(TOKEN_KEY);
            await _localStorage.RemoveItemAsync(REFRESH_TOKEN_KEY);
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
            return !string.IsNullOrEmpty(token);
        }

        public async Task<string> GetTokenAsync()
        {
            var token = await _localStorage.GetItemAsync<string>(TOKEN_KEY);
            return token ?? string.Empty;
        }
    }

    public class LoginResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
