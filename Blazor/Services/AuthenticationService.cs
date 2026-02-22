using DomainModels;
using Microsoft.JSInterop;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace Blazor.Services
{
    /// <summary>
    /// Service til håndtering af authentication, JWT tokens og bruger state
    /// </summary>
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly APIService _apiService;

        public event EventHandler<bool>? AuthenticationStateChanged;

        private const string TOKEN_KEY = "authToken";
        private const string USER_KEY = "currentUser";

        public AuthenticationService(HttpClient httpClient, IJSRuntime jsRuntime, APIService apiService)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _apiService = apiService;
        }

        /// <summary>
        /// Logger brugeren ind og gemmer token + brugerinfo
        /// </summary>
        public async Task<LoginResult> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var apiResult = await _apiService.LoginAsync(loginDto);
                
                if (apiResult.Success && apiResult.Response?.Token != null && apiResult.Response.User != null)
                {
                    // Gem token i localStorage
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", TOKEN_KEY, apiResult.Response.Token);
                    
                    // Gem bruger info i localStorage
                    var userJson = JsonSerializer.Serialize(apiResult.Response.User);
                    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", USER_KEY, userJson);

                    // Sæt Authorization header for fremtidige requests
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiResult.Response.Token);

                    // Notify listeners om authentication state change
                    AuthenticationStateChanged?.Invoke(this, true);

                    return new LoginResult 
                    { 
                        Success = true, 
                        Message = apiResult.Response.Message,
                        User = apiResult.Response.User,
                        Token = apiResult.Response.Token
                    };
                }
                else if (apiResult.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    return new LoginResult 
                    { 
                        Success = false, 
                        Message = apiResult.ErrorResponse?.Message ?? "For mange login forsøg. Prøv igen senere.",
                        RemainingLockoutSeconds = apiResult.ErrorResponse?.RemainingLockoutSeconds ?? 0
                    };
                }
                else
                {
                    return new LoginResult 
                    { 
                        Success = false, 
                        Message = apiResult.ErrorResponse?.Message ?? apiResult.ErrorMessage ?? "Login fejlede. Tjek dine credentials.",
                        DelayApplied = apiResult.ErrorResponse?.DelayApplied ?? 0
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login fejl: {ex.Message}");
                return new LoginResult { Success = false, Message = "Der opstod en fejl under login. Prøv igen." };
            }
        }

        /// <summary>
        /// Logger brugeren ud og rydder token + brugerinfo
        /// </summary>
        public async Task LogoutAsync()
        {
            try
            {
                // Fjern token fra localStorage
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", TOKEN_KEY);
                
                // Fjern bruger info fra localStorage
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", USER_KEY);

                // Fjern Authorization header
                _httpClient.DefaultRequestHeaders.Authorization = null;

                // Notify listeners om authentication state change
                AuthenticationStateChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Logout fejl: {ex.Message}");
            }
        }

        /// <summary>
        /// Tjekker om brugeren er logget ind
        /// </summary>
        public async Task<bool> IsAuthenticatedAsync()
        {
            try
            {
                var token = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TOKEN_KEY);
                
                if (string.IsNullOrEmpty(token))
                    return false;

                // Tjek om token er udløbet (basic check)
                if (IsTokenExpired(token))
                {
                    await LogoutAsync();
                    return false;
                }

                // Sæt Authorization header hvis ikke allerede sat
                if (_httpClient.DefaultRequestHeaders.Authorization == null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Authentication check fejl: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Henter nuværende bruger fra localStorage
        /// </summary>
        public async Task<AuthUserInfo?> GetCurrentUserAsync()
        {
            try
            {
                var userJson = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", USER_KEY);
                
                if (string.IsNullOrEmpty(userJson))
                    return null;

                return JsonSerializer.Deserialize<AuthUserInfo>(userJson, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get current user fejl: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Henter token fra localStorage
        /// </summary>
        public async Task<string?> GetTokenAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", TOKEN_KEY);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get token fejl: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Initialiserer authentication state ved app start
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                var token = await GetTokenAsync();
                if (!string.IsNullOrEmpty(token) && !IsTokenExpired(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    AuthenticationStateChanged?.Invoke(this, true);
                }
                else if (!string.IsNullOrEmpty(token))
                {
                    // Token er udløbet, log ud
                    await LogoutAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Initialize authentication fejl: {ex.Message}");
            }
        }

        /// <summary>
        /// Basic check for om JWT token er udløbet
        /// </summary>
        private bool IsTokenExpired(string token)
        {
            try
            {
                // Split JWT token (header.payload.signature)
                var parts = token.Split('.');
                if (parts.Length != 3)
                    return true;

                // Decode payload (base64url)
                var payload = parts[1];
                
                // Pad base64 string if needed
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                // Replace URL-safe characters
                payload = payload.Replace('-', '+').Replace('_', '/');

                var jsonBytes = Convert.FromBase64String(payload);
                var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
                
                using var document = JsonDocument.Parse(jsonString);
                
                if (document.RootElement.TryGetProperty("exp", out var expElement))
                {
                    var exp = expElement.GetInt64();
                    var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp);
                    return DateTime.UtcNow >= expDateTime.DateTime;
                }

                return true; // Hvis vi ikke kan læse exp, antag token er udløbet
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Token expiry check fejl: {ex.Message}");
                return true; // Ved fejl, antag token er udløbet
            }
        }
    }




}
