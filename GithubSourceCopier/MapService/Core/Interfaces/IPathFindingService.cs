using GithubSourceCopier.MapService.Core.Models;

namespace GithubSourceCopier.MapService.Core.Interfaces;


/// <summary>
/// Provides algorithms for finding optimal paths using API data.
/// </summary>
public interface IPathFindingService
{
    Task<PathModel> FindShortestPathAsync(string origin, string destination);
    Task<string> FindDetailedRouteAsync(string origin, string destination);
    Task<object> FindRouteForMapAsync(string origin, string destination);
}