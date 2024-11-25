using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;

namespace BlazorWASM.Services
{
    public interface IGoogleAuthService
    {
        Task<bool> LoginAsync(string idToken);
        Task LogoutAsync();
    }

    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;

        public GoogleAuthService(HttpClient httpClient, ILocalStorageService localStorage, NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        public async Task<bool> LoginAsync(string idToken)
        {
            var response = await _httpClient.PostAsJsonAsync("api/GoogleAuth/login", new { idToken });

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<GoogleLoginResponse>();
                await _localStorage.SetItemAsync("authToken", result.Token);
                await _localStorage.SetItemAsync("user", result.User);
                _navigationManager.NavigateTo("/", forceLoad: true);
                return true;
            }

            return false;
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("user");
            _navigationManager.NavigateTo("/", forceLoad: true);
        }
    }

    public class GoogleLoginResponse
    {
        public string Token { get; set; }
        public object User { get; set; }
    }
} 