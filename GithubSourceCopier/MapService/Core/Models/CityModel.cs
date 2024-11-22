namespace GithubSourceCopier.MapService.Core.Models;

/// <summary>
/// Represents city information obtained from API.
/// </summary>
public class CityModel
{
    public string Name { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
