namespace GithubSourceCopier.Models;

public class ProjectUpdateRequest
{
    public string GitHubLink { get; set; }
    public string LocalPath { get; set; }
    public string OldVersion { get; set; }
    public string NewVersion { get; set; }
}
