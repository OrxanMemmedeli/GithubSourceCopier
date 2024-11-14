namespace GithubSourceCopier.Services;

public interface IProjectUpdaterService
{
    IAsyncEnumerable<string> DownloadAndCopyFilesAsync(string githubUrl, string localPath, string oldVersion, string newVersion, string targetNamespace);
}
