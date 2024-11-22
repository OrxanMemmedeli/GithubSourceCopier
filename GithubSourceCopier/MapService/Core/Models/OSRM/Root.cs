namespace GithubSourceCopier.MapService.Core.Models.OSRM
{
    public class Root
    {
        public string code { get; set; }
        public Route[] routes { get; set; }
        public Waypoint[] waypoints { get; set; }
    }
}
