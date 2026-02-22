namespace Blazor.Services
{
    public partial class APIService
    {
        protected readonly HttpClient _httpClient;

        public APIService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
    }
}
