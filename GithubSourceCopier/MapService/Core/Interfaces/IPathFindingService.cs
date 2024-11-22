using GithubSourceCopier.MapService.Core.Models;

namespace GithubSourceCopier.MapService.Core.Interfaces;


/// <summary>
/// Provides algorithms for finding optimal paths in a graph.
/// </summary>
public interface IPathFindingService
{
    Task<PathModel> FindShortestPathAsync(string source, string destination);
}