using System;
using System.Net.Http;
using Blazor.Services;
using Blazor.Models;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Blazor;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        // Læs API endpoint fra miljøvariabler eller brug default
        var envApiEndpoint = Environment.GetEnvironmentVariable("API_ENDPOINT");
        Console.WriteLine($"API ENV Endpoint: {envApiEndpoint}");
        var apiEndpoint = envApiEndpoint ?? "https://25h2-mags.mercantec.tech/";
        Console.WriteLine($"API Endpoint: {apiEndpoint}");

        // Registrer HttpClient til API service med konfigurerbar endpoint
        builder.Services.AddHttpClient<APIService>(client =>
        {
            client.BaseAddress = new Uri(apiEndpoint);
            Console.WriteLine($"APIService BaseAddress: {client.BaseAddress}");
        });

        // Registrer HttpClient til AuthenticationService (shared med APIService)
        builder.Services.AddHttpClient<AuthenticationService>(client =>
        {
            client.BaseAddress = new Uri(apiEndpoint);
        });

        // Registrer AuthenticationService som Scoped service
        builder.Services.AddScoped<AuthenticationService>();
        
        // Registrer TicketSignalRService som Scoped service
        builder.Services.AddScoped<TicketSignalRService>();
        
        // Konfigurer API base URL for SignalR
        builder.Services.Configure<ApiConfiguration>(config =>
        {
            config.ApiBaseUrl = apiEndpoint;
        });

        var app = builder.Build();

        // Initialiser authentication state
        var authService = app.Services.GetRequiredService<AuthenticationService>();
        await authService.InitializeAsync();

        await app.RunAsync();
    }
}
