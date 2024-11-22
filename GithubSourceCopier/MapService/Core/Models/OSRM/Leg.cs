namespace GithubSourceCopier.MapService.Core.Models.OSRM
{
    public class Leg
    {
        public Step[] Steps { get; set; }
        public string summary { get; set; }
        public double weight { get; set; }
        public double duration { get; set; }
        public double distance { get; set; }
    }
}
