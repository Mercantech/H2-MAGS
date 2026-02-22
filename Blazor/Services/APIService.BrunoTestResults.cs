using DomainModels;
using System.Net.Http.Json;

namespace Blazor.Services;

public partial class APIService
{
    /// <summary>
    /// Henter oversigt over alle tilgængelige test resultater
    /// </summary>
    public async Task<TestResultsOverview?> GetBrunoTestResultsOverviewAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<TestResultsOverview>("api/BrunoTestResults/overview");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved hentning af Bruno test resultat oversigt: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Henter det seneste test resultat i JSON format
    /// </summary>
    public async Task<BrunoTestResult?> GetLatestBrunoTestResultAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BrunoTestResult>("api/BrunoTestResults/latest");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved hentning af seneste Bruno test resultat: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Henter et specifikt test resultat baseret på filnavn
    /// </summary>
    public async Task<BrunoTestResult?> GetBrunoTestResultByFilenameAsync(string filename)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BrunoTestResult>($"api/BrunoTestResults/file/{filename}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved hentning af Bruno test resultat fil {filename}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Henter HTML rapport som tekst
    /// </summary>
    public async Task<string?> GetBrunoTestHtmlReportAsync(string filename)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/BrunoTestResults/report/{filename}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved hentning af HTML rapport {filename}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Henter test statistikker
    /// </summary>
    public async Task<object?> GetBrunoTestStatsAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<object>("api/BrunoTestResults/stats");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved hentning af Bruno test statistikker: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Rydder op i gamle test resultater
    /// </summary>
    public async Task<bool> CleanupBrunoTestResultsAsync()
    {
        try
        {
            var response = await _httpClient.DeleteAsync("api/BrunoTestResults/cleanup");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fejl ved oprydning af Bruno test resultater: {ex.Message}");
            return false;
        }
    }
}
