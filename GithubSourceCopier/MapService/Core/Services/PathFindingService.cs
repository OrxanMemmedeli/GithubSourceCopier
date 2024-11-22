using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Models;

namespace GithubSourceCopier.MapService.Core.Services;

/// <summary>
/// Provides algorithms for finding the shortest path between two nodes.
/// </summary>
public class PathFindingService : IPathFindingService
{
    private readonly IGraphService _graphService;

    public PathFindingService(IGraphService graphService)
    {
        _graphService = graphService;
    }

    public async Task<PathModel> FindShortestPathAsync(string source, string destination)
    {
        // Implement Dijkstra or Neo4j-specific shortest path logic here.
        throw new NotImplementedException();
    }
}
