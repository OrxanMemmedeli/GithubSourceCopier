using GithubSourceCopier.MapService.Core.Models;

namespace GithubSourceCopier.MapService.Core.Interfaces;

/// <summary>
/// Provides methods for managing cities and their connections in a graph database.
/// </summary>
public interface IGraphService
{
    Task AddCityAsync(CityModel city);
    Task<IEnumerable<CityModel>> GetCitiesAsync(string filter);
    Task AddPathAsync(PathModel path);
    Task<IEnumerable<PathModel>> GetPathsAsync(string source, string destination);
}

