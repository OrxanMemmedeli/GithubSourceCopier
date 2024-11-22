using GithubSourceCopier.MapService.Core.Interfaces;
using GithubSourceCopier.MapService.Core.Models;
using Neo4j.Driver;

namespace GithubSourceCopier.MapService.Core.Services;

/// <summary>
/// Manages cities and their connections using Neo4j.
/// </summary>
public class GraphService : IGraphService
{
    private readonly IDriver _driver;

    public GraphService(IDriver driver)
    {
        _driver = driver;
    }

    public async Task AddCityAsync(CityModel city)
    {
        using var session = _driver.AsyncSession();
        await session.RunAsync("CREATE (c:City { Name: $name, Population: $population })",
            new { name = city.Name, population = city.Population });
    }

    public async Task<IEnumerable<CityModel>> GetCitiesAsync(string filter)
    {
        using var session = _driver.AsyncSession();
        var result = await session.RunAsync("MATCH (c:City) WHERE c.Name CONTAINS $filter RETURN c",
            new { filter });
        return await result.ToListAsync(r => new CityModel
        {
            Name = r["c"].As<INode>().Properties["Name"].As<string>(),
            Population = r["c"].As<INode>().Properties["Population"].As<int>()
        });
    }

    public async Task AddPathAsync(PathModel path)
    {
        using var session = _driver.AsyncSession();
        await session.RunAsync(
            "MATCH (a:City { Name: $source }), (b:City { Name: $destination }) " +
            "CREATE (a)-[:CONNECTED { Distance: $distance }]->(b)",
            new { source = path.Source, destination = path.Destination, distance = path.Distance });
    }

    public async Task<IEnumerable<PathModel>> GetPathsAsync(string source, string destination)
    {
        using var session = _driver.AsyncSession();
        var result = await session.RunAsync(
            "MATCH (a:City { Name: $source })-[r:CONNECTED]->(b:City { Name: $destination }) RETURN r",
            new { source, destination });
        return await result.ToListAsync(r => new PathModel
        {
            Source = source,
            Destination = destination,
            Distance = r["r"].As<IRelationship>().Properties["Distance"].As<double>()
        });
    }
}
