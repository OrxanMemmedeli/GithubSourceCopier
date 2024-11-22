namespace GithubSourceCopier.MapService.Core.Models.OSRM
{
    public class Maneuver
    {
        public int bearing_after { get; set; }
        public int bearing_before { get; set; }
        public double[] location { get; set; }
        public string modifier { get; set; }
        public string type { get; set; }
        public int? exit { get; set; }
    }
}
