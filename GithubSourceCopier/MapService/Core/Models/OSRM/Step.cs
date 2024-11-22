namespace GithubSourceCopier.MapService.Core.Models.OSRM
{
    public class Step
    {
        public Geometry geometry { get; set; }
        public Maneuver maneuver { get; set; }
        public string mode { get; set; }
        public string driving_side { get; set; }
        public string name { get; set; }
        public Intersection[] intersections { get; set; }
        public double weight { get; set; }
        public double duration { get; set; }
        public double distance { get; set; }
        public string rotary_name { get; set; }
        public string @ref { get; set; }
    }
}
