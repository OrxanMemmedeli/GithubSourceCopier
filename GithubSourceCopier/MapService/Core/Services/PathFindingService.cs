using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Models;
using GithubSourceCopier.MapService.Core.Models.Google;
using GithubSourceCopier.MapService.Core.Models.OSRM;

namespace GithubSourceCopier.MapService.Core.Services;

/// <summary>
/// Provides pathfinding capabilities using Google Maps Directions API.
/// </summary>
public class PathFindingService : IPathFindingService
{
    private readonly HttpClient _httpClient;
    private readonly string _googleMapsApiKey;

    public PathFindingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _googleMapsApiKey = "AIzaSyDIdNDh9o3JvMuAyGgEzf0bZiV_DtljyV4";
    }

    public async Task<PathModel> FindShortestPathAsync(string origin, string destination)
    {
        //return await WithGoogle(origin, destination);

        return await WithOSMR(origin, destination);
    }

    private async Task<PathModel> WithOSMR(string origin, string destination)
    {
        _httpClient.BaseAddress = new Uri("http://router.project-osrm.org/");

        // OSRM API çağırışı üçün URL
        var url = $"route/v1/driving/{origin};{destination}?steps=true&overview=full";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API error: {response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<Root>();

        if (data != null && data.routes?.Length > 0)
        {
            var route = data.routes[0];
            var leg = route.legs[0];

            // Marşrut detalları
            var steps = leg.Steps.Select(step => new
            {
                Instruction = step.maneuver.type + (string.IsNullOrEmpty(step.maneuver.modifier) ? "" : $" ({step.maneuver.modifier})"),
                Street = string.IsNullOrEmpty(step.name) ? "Unnamed Road" : step.name,
                Distance = step.distance,
                Duration = step.duration
            });

            return new PathModel
            {
                Origin = origin,
                Destination = destination,
                Distance = route.distance / 1000.0, // Məsafəni kilometrə çevir
                RouteDetails = string.Join("\n\r", steps.Select(s => $"\n\rGo {s.Instruction} on {s.Street} for {s.Distance:F2} meters. Estimated time: {s.Duration / 60.0:F2} minutes.\n\r"))
            };
        }

        return null;
    }


    private async Task<PathModel> WithGoogle(string origin, string destination)
    {
        var url = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin}&destination={destination}&key={_googleMapsApiKey}";

        // API-dən cavabı götür
        var response = await _httpClient.GetFromJsonAsync<GoogleDirectionsResponse>(url);

        if (response != null && response.Routes?.Length > 0)
        {
            var route = response.Routes[0].Legs[0];

            return new PathModel
            {
                Origin = origin,
                Destination = destination,
                Distance = route.Distance.Value / 1000.0, // Məsafəni kilometrə çevir
                RouteDetails = string.Join(", ", route.Steps.Select(s => s.HtmlInstructions))
            };
        }

        return null; // Əgər heç bir marşrut tapılmazsa
    }

    public async Task<string> FindDetailedRouteAsync(string origin, string destination)
    {
        var originCoords = await GetCoordinatesAsync(origin);
        var destinationCoords = await GetCoordinatesAsync(destination);

        _httpClient.BaseAddress = new Uri("http://router.project-osrm.org/");

        var url = $"route/v1/driving/{originCoords};{destinationCoords}?steps=true&overview=full";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new Exception("OSRM server returned 403 Forbidden. Please check server availability or your IP restrictions.");
            }
            throw new Exception($"API error: {response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<Root>();

        if (data != null && data.routes?.Length > 0)
        {
            var route = data.routes[0];
            var leg = route.legs[0];

            var steps = leg.Steps.Select(step =>
                $"Turn {step.maneuver.type} onto {(!string.IsNullOrEmpty(step.name) ? step.name : "Unnamed Road")} for {step.distance:F2} meters. Duration: {step.duration / 60.0:F2} minutes.");

            return string.Join("\n", steps);
        }

        return "No route found.";
    }


    public async Task<object> FindRouteForMapAsync(string origin, string destination)
    {
        var originCoords = await GetCoordinatesAsync(origin);
        var destinationCoords = await GetCoordinatesAsync(destination);

        _httpClient.BaseAddress = new Uri("http://router.project-osrm.org/");

        var url = $"route/v1/driving/{originCoords};{destinationCoords}?steps=true&overview=full";

        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"API error: {response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<Root>();

        if (data != null && data.routes?.Length > 0)
        {
            var route = data.routes[0];
            var leg = route.legs[0];

            // Marşrut nöqtələrini xəritədə göstərmək üçün hazırlayın
            var coordinates = leg.Steps.Select(step => new
            {
                Latitude = step.geometry.coordinates[0][1], // Latitute
                Longitude = step.geometry.coordinates[0][0] // Longitude
            });

            return new
            {
                Origin = origin,
                Destination = destination,
                RouteCoordinates = coordinates,
                TotalDistance = route.distance / 1000.0,
                TotalDuration = route.duration / 60.0
            };
        }

        return new { Message = "No route found." };
    }
    private async Task<string> GetCoordinatesAsync(string address)
    {
        var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<List<OpenStreetMapCityResponse>>(url);

            if (response != null && response.Count > 0)
            {
                var city = response.First();
                return $"{city.Lat},{city.Lon}";
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Error fetching coordinates from Nominatim: {ex.Message}");
        }

        throw new Exception($"Address '{address}' could not be converted to coordinates.");
    }


    //private async Task<string> GetCoordinatesAsync(string address)
    //{
    //    var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(address)}&format=json&limit=1";

    //    // User-Agent başlığını əlavə edin
    //    _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("YourAppName/1.0 (your-email@example.com)");

    //    var response = await _httpClient.GetAsync(url);

    //    if (!response.IsSuccessStatusCode)
    //    {
    //        throw new Exception($"Nominatim API error: {response.StatusCode}");
    //    }

    //    var data = await response.Content.ReadFromJsonAsync<List<OpenStreetMapCityResponse>>();

    //    if (data != null && data.Count > 0)
    //    {
    //        var city = data.First();
    //        return $"{city.Lat},{city.Lon}"; // Latitude və Longitude qaytarır
    //    }

    //    throw new Exception($"Address '{address}' could not be converted to coordinates.");
    //}



}