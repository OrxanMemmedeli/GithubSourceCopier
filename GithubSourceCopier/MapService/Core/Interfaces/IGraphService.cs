using GithubSourceCopier.MapService.Core.Models;

namespace GithubSourceCopier.MapService.Core.Interfaces;

/// <summary>
/// Provides methods for managing cities and their connections using API requests.
/// </summary>
public interface IGraphService
{
    Task<CityModel> GetCityAsync(string cityName);
    Task<IEnumerable<CityModel>> SearchCitiesAsync(string filter);
}

