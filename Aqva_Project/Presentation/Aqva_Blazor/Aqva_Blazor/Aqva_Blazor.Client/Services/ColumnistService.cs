using System.Text.Json;
using Aqva_Blazor.Client.Models;

namespace Aqva_Blazor.Client.Services;

public class ColumnistService(HttpClient httpClient)
{
    private const string BaseUrl = "http://localhost:5044/search";

    public async Task<List<Columnist>> LoadAllColumnistsAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"{BaseUrl}/all");
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Columnist>>(responseBody) ?? new List<Columnist>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON Error: {ex.Message}");
            return new List<Columnist>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            return new List<Columnist>();
        }
    }

    public async Task<List<Columnist>> SearchColumnistsAsync(string query)
    {
        try
        {
            var encodedQuery = Uri.EscapeDataString(query);
            var url = $"{BaseUrl}?query={encodedQuery}";

            var response = await httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<Columnist>>(responseBody) ?? new List<Columnist>();
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"JSON Error: {ex.Message}");
            return new List<Columnist>();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Request error: {ex.Message}");
            return new List<Columnist>();
        }
    }
}