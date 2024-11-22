using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Models;
using Neo4j.Driver;
using System.Net.Http.Headers;

namespace GithubSourceCopier.MapService.Core.Services;

/// <summary>
/// Manages city-related data using OpenStreetMap Overpass API.
/// </summary>
public class GraphService : IGraphService
{
    private readonly HttpClient _httpClient;

    public GraphService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("MapService", "1.0"));
    }

    public async Task<CityModel> GetCityAsync(string cityName)
    {
        // API endpoint URL
        var url = $"https://nominatim.openstreetmap.org/search?q={cityName}&format=json&limit=1";

        // API sorğusunu göndərmək
        var response = await _httpClient.GetAsync(url);

        // Uğursuz sorğular üçün istisna
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"API request failed with status code {response.StatusCode}");

        // JSON cavabını deserializasiya
        var cities = await response.Content.ReadFromJsonAsync<List<OpenStreetMapCityResponse>>();

        // Heç bir nəticə tapılmadığı halda istisna
        if (cities == null || cities.Count == 0)
            throw new Exception($"No results found for city: {cityName}");

        // İlk şəhəri götür və CityModel formatına çevir
        var firstCity = cities.First();

        return new CityModel
        {
            Name = firstCity.Display_name,
            Latitude = double.Parse(firstCity.Lat),
            Longitude = double.Parse(firstCity.Lon)
        };
    }

    public async Task<IEnumerable<CityModel>> SearchCitiesAsync(string filter)
    {
        // API endpoint URL
        var url = $"https://nominatim.openstreetmap.org/search?q={filter}&format=json&limit=5";

        // API sorğusunu göndərmək
        var response = await _httpClient.GetAsync(url);

        // Uğursuz sorğular üçün istisna
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"API request failed with status code {response.StatusCode}");
        }

        // JSON cavabını deserializasiya
        var cities = await response.Content.ReadFromJsonAsync<List<OpenStreetMapCityResponse>>();

        // Nəticə tapılmadığı halda boş siyahı qaytarılır
        if (cities == null || cities.Count == 0)
        {
            return Enumerable.Empty<CityModel>();
        }

        // JSON cavabını `CityModel` obyektlərinə çevir və qaytar
        return cities.Select(city => new CityModel
        {
            Name = city.Display_name,
            Latitude = double.Parse(city.Lat),
            Longitude = double.Parse(city.Lon)
        });
    }
}