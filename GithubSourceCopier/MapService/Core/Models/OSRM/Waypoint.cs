namespace GithubSourceCopier.MapService.Core.Models.OSRM
{
    public class Waypoint
    {
        public string hint { get; set; }
        public double distance { get; set; }
        public string name { get; set; }
        public double[] location { get; set; }
    }
}
