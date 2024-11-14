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
        // Ana qovluq üçün rekursiv oxuma prosesini başladırıq
        await ProcessDirectoryAsync(githubUrl, localPath, oldVersion, newVersion);
    }

    private async Task ProcessDirectoryAsync(string apiUrl, string localPath, string oldVersion, string newVersion)
    {
        // GitHub API-dən qovluğun məzmununu çəkirik
        var response = await _httpClient.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var files = JsonSerializer.Deserialize<IEnumerable<GithubFile>>(content);

        foreach (var file in files)
        {
            if (file.Type == "file" && file.DownloadUrl != null)
            {
                // Əgər obyekt fayldırsa, faylı endiririk
                var fileContent = await DownloadFileContentAsync(file);

                // Versiya uyğunluğunu təmin etmək üçün məzmunu dəyişirik
                fileContent = UpdateVersionCompatibility(fileContent, oldVersion, newVersion);

                // Faylı yerli qovluqda saxlayırıq
                var destinationPath = Path.Combine(localPath, file.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty); // Qovluqları yaradırıq
                await File.WriteAllTextAsync(destinationPath, fileContent);
            }
            else if (file.Type == "dir" && file.Url != null)
            {
                // Əgər obyekt qovluqdursa, həmin qovluğu yerli olaraq yaradıb içini rekursiv şəkildə oxuyuruq
                var subDirectoryPath = Path.Combine(localPath, file.Name);
                Directory.CreateDirectory(subDirectoryPath); // Yerli qovluq yaradırıq

                // Alt qovluğun məzmununu oxumaq üçün rekursiv çağırış
                await ProcessDirectoryAsync(file.Url, subDirectoryPath, oldVersion, newVersion);
            }
        }
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
}
