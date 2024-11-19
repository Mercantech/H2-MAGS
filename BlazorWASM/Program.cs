using Blazored.LocalStorage;
using BlazorWASM.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace BlazorWASM;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Tilføj LocalStorage service
        builder.Services.AddBlazoredLocalStorage();

        // Tilføj dine andre services
        builder.Services.AddScoped<IAuthService, AuthService>();

        // Konfigurer HttpClient med base URL
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(
                Environment.GetEnvironmentVariable("API_BASE_URL")
                    ?? "https://h2-mags.onrender.com/"
            )
        });

        await builder.Build().RunAsync();
    }
}
