namespace GithubSourceCopier.MapService.Core.Models.OSRM
{
    public class Intersection
    {
        public int @out { get; set; }
        public bool[] entry { get; set; }
        public int[] bearings { get; set; }
        public double[] location { get; set; }
        public int? @in { get; set; }
    }
}
