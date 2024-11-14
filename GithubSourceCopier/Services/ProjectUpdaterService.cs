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

    public async IAsyncEnumerable<string> DownloadAndCopyFilesAsync(string githubUrl, string localPath, string oldVersion, string newVersion, string targetNamespace)
    {
        var addedFiles = new List<string>();

        // GitHub URL-ni API formatına uyğunlaşdırırıq
        var apiUrl = githubUrl
            .Replace("github.com", "api.github.com/repos") // github.com -> api.github.com/repos
            .Replace("/tree/master/", "/contents/");       // tree/master -> contents

        // Ana qovluq üçün rekursiv oxuma prosesini başladırıq
        await foreach (var filePath in ProcessDirectoryAsync(apiUrl, localPath, oldVersion, newVersion, targetNamespace))
        {
            yield return filePath; // Hər faylı dərhal qaytarırıq
        }
    }

    private async IAsyncEnumerable<string> ProcessDirectoryAsync(string apiUrl, string localPath, string oldVersion, string newVersion, string targetNamespace)
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

        var response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"GitHub-dan məlumatları əldə etmək alınmadı: {response.StatusCode} - {response.ReasonPhrase}");
        }

        var content = await response.Content.ReadAsStringAsync();


        var files = JsonSerializer.Deserialize<IEnumerable<GithubFile>>(content);

        var withoutCLFile = files.Where(x => !x.Name.EndsWith(".csproj"));
        foreach (var file in withoutCLFile)
        {
            if (file.Type == "file" && file.DownloadUrl != null)
            {
                var fileContent = await DownloadFileContentAsync(file);

                // Versiya uyğunluğunu təmin etmək üçün məzmunu dəyişirik
                fileContent = UpdateVersionCompatibility(fileContent, oldVersion, newVersion);

                // Faylın namespace hissəsini yeniləyirik
                var updatedNamespace = GenerateNamespace(localPath, targetNamespace);
                fileContent = UpdateNamespace(fileContent, updatedNamespace);

                // Faylı yerli qovluqda saxlayırıq
                var destinationPath = Path.Combine(localPath, file.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);
                await File.WriteAllTextAsync(destinationPath, fileContent);

                yield return destinationPath; // Faylın yolunu dərhal qaytarırıq
            }
            else if (file.Type == "dir" && file.Url != null)
            {
                var subDirectoryPath = Path.Combine(localPath, file.Name);
                Directory.CreateDirectory(subDirectoryPath);

                await foreach (var subFilePath in ProcessDirectoryAsync(file.Url, subDirectoryPath, oldVersion, newVersion, targetNamespace))
                {
                    yield return subFilePath; // Alt qovluq fayllarını da dərhal qaytarırıq
                }
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
        return content.Replace(oldVersion, newVersion);
    }

    private string UpdateNamespace(string content, string targetNamespace)
    {
        var namespaceLine = "namespace ";
        var startIndex = content.IndexOf(namespaceLine);

        if (startIndex >= 0)
        {
            var endIndex = content.IndexOf("\n", startIndex);
            content = content.Remove(startIndex, endIndex - startIndex).Insert(startIndex, $"{namespaceLine}{targetNamespace};");
        }

        return content;
    }

    private string GenerateNamespace(string localPath, string targetNamespace)
    {
        // `Entities` qovluğunu tapırıq və yalnız ondan sonrakı yolu namespace kimi istifadə edirik
        var entitiesIndex = localPath.IndexOf("Entities", StringComparison.OrdinalIgnoreCase);
        var relativePath = entitiesIndex >= 0 ? localPath[(entitiesIndex + "Entities".Length)..].Trim(Path.DirectorySeparatorChar) : string.Empty;

        relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '.');
        return $"{targetNamespace}.Entities{(string.IsNullOrEmpty(relativePath) ? string.Empty : "." + relativePath)}";
    }

}
