using GithubSourceCopier.Models;
using System.Text.Json;

namespace GithubSourceCopier.Services;

public class ProjectUpdaterService : IProjectUpdaterService
{
    private readonly HttpClient _httpClient;

    public ProjectUpdaterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DownloadAndCopyFilesAsync(string githubUrl, string localPath, string oldVersion, string newVersion)
    {
        // GitHub repo-dan faylları endir
        var files = await GetFilesFromGithub(githubUrl);

        foreach (var file in files)
        {
            // Yalnız fayl tipində olan obyektləri (dirs olmayan) yükləyirik
            if (file.DownloadUrl != null)
            {
                var content = await DownloadFileContentAsync(file);

                // Köhnə versiyadan yeni versiyaya uyğunluğu təmin et
                content = UpdateVersionCompatibility(content, oldVersion, newVersion);

                var destinationPath = Path.Combine(localPath, file.Name);
                await File.WriteAllTextAsync(destinationPath, content);
            }
            else
            {
                Console.WriteLine($"Qovluq keçildi: {file.Name}");
            }
        }
    }


    private async Task<IEnumerable<GithubFile>> GetFilesFromGithub(string githubUrl)
    {
        // GitHub URL-ni API formatına uyğunlaşdırırıq
        var apiUrl = githubUrl
            .Replace("github.com", "api.github.com/repos")
            .Replace("/tree/master/", "/contents/");

        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

        var response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"GitHub-dan məlumatları əldə etmək alınmadı: {response.StatusCode} - {response.ReasonPhrase}");

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<GithubFile>>(content);
    }


    private async Task<string> DownloadFileContentAsync(GithubFile file)
    {
        var response = await _httpClient.GetAsync(file.DownloadUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private string UpdateVersionCompatibility(string content, string oldVersion, string newVersion)
    {
        // Kodun köhnə versiyasını yeni versiyaya uyğunlaşdırmaq üçün dəyişikliklər
        return content.Replace(oldVersion, newVersion);
    }
}