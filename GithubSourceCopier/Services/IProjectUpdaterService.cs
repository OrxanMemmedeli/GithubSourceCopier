namespace GithubSourceCopier.Services;

public interface IProjectUpdaterService
{
    Task DownloadAndCopyFilesAsync(string githubUrl, string localPath, string oldVersion, string newVersion);
}
