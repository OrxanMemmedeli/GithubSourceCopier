namespace GithubSourceCopier.MapService.Core.Models;

/// <summary>
/// Represents a path between two cities.
/// </summary>
public class PathModel
{
    public string Origin { get; set; }
    public string Destination { get; set; }
    public double Distance { get; set; }
    public string RouteDetails { get; set; }
}
