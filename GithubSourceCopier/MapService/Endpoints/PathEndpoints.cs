using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Models;

namespace GithubSourceCopier.MapService.Endpoints;

public static class PathEndpoints
{
    public static void MapPathEndpoints(this WebApplication app)
    {
        app.MapPost("/path", async (PathModel path, IGraphService service) =>
        {
            await service.AddPathAsync(path);
            return Results.Ok();
        });

        app.MapGet("/path/{source}/{destination}", async (string source, string destination, IGraphService service) =>
        {
            var paths = await service.GetPathsAsync(source, destination);
            return Results.Ok(paths);
        });

        app.MapGet("/path/shortest", async (string source, string destination, IPathFindingService service) =>
        {
            var shortestPath = await service.FindShortestPathAsync(source, destination);
            return Results.Ok(shortestPath);
        });
    }
}