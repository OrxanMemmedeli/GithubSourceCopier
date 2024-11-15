using GithubSourceCopier.Models;
using System.Text.Json;

namespace GithubSourceCopier.Services;

/// <summary>
/// Service for updating project files from a GitHub repository to a local path.
/// <br/>Proyektdə GitHub repository-dən faylları yerli bir qovluğa əlavə etmək üçün xidmət.
/// </summary>
public class ProjectUpdaterService : IProjectUpdaterService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectUpdaterService"/> class.
    /// <br/>ProjectUpdaterService sinifini başlatmaq üçün konstruktor.
    /// </summary>
    /// <param name="httpClient">The HTTP client instance.<br/>HTTP müştəri nümunəsi.</param>
    public ProjectUpdaterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<string> DownloadAndCopyFilesAsync(string githubUrl, string localPath, string oldVersion, string newVersion, string targetNamespace)
    {
        // Convert GitHub URL to API-compatible URL
        // GitHub URL-ni API ilə uyğun formata çeviririk
        var apiUrl = githubUrl
            .Replace("github.com", "api.github.com/repos") // github.com -> api.github.com/repos
            .Replace("/tree/master/", "/contents/");       // tree/master -> contents

        // Start recursive processing for the main directory
        // Əsas qovluq üçün rekursiv oxuma prosesini başladırıq
        await foreach (var filePath in ProcessDirectoryAsync(apiUrl, localPath, oldVersion, newVersion, targetNamespace))
        {
            yield return filePath; // Return each processed file immediately (Hər işlənmiş faylı dərhal qaytarırıq)
        }
    }

    /// <summary>
    /// Recursively processes directories and downloads files.
    /// Qovluqları rekursiv şəkildə oxuyur və faylları endirir.
    /// </summary>
    private async IAsyncEnumerable<string> ProcessDirectoryAsync(string apiUrl, string localPath, string oldVersion, string newVersion, string targetNamespace)
    {
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("request");

        // Send GET request to GitHub API
        // GitHub API-yə GET sorğusu göndəririk
        var response = await _httpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"GitHub-dan məlumatları əldə etmək alınmadı: {response.StatusCode} - {response.ReasonPhrase}"); // GitHub-dan məlumat almaq alınmadı.
        }

        var content = await response.Content.ReadAsStringAsync();

        // Deserialize JSON response into GithubFile objects
        // JSON cavabını GithubFile obyektlərinə deserializasiya edirik
        var files = JsonSerializer.Deserialize<IEnumerable<GithubFile>>(content);

        var withoutCLFile = files.Where(x => !x.Name.EndsWith(".csproj"));
        foreach (var file in withoutCLFile)
        {
            if (file.Type == "file" && file.DownloadUrl != null)
            {
                var fileContent = await DownloadFileContentAsync(file);

                // Update content for version compatibility
                // Məzmunu versiya uyğunluğu üçün yeniləyirik
                fileContent = UpdateVersionCompatibility(fileContent, oldVersion, newVersion);

                // Update namespace in the file
                // Faylda namespace hissəsini yeniləyirik
                var updatedNamespace = GenerateNamespace(localPath, targetNamespace);
                fileContent = UpdateNamespace(fileContent, updatedNamespace);

                // Save the file to the local path
                // Faylı yerli qovluğa yazırıq
                var destinationPath = Path.Combine(localPath, file.Name);
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);
                await File.WriteAllTextAsync(destinationPath, fileContent);

                yield return destinationPath; // Return the file path
                // Fayl yolunu qaytarırıq
            }
            else if (file.Type == "dir" && file.Url != null)
            {
                // Process subdirectory recursively
                // Alt qovluqları rekursiv şəkildə oxuyuruq
                var subDirectoryPath = Path.Combine(localPath, file.Name);
                Directory.CreateDirectory(subDirectoryPath);

                await foreach (var subFilePath in ProcessDirectoryAsync(file.Url, subDirectoryPath, oldVersion, newVersion, targetNamespace))
                {
                    yield return subFilePath; // Return files from subdirectories
                    // Alt qovluqlardan faylları qaytarırıq
                }
            }
        }

    }


    /// <summary>
    /// Downloads the content of a file from GitHub.
    /// GitHub-dan bir faylın məzmununu yükləyir.
    /// </summary>
    private async Task<string> DownloadFileContentAsync(GithubFile file)
    {
        var response = await _httpClient.GetAsync(file.DownloadUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }


    /// <summary>
    /// Updates file content for version compatibility.
    /// Fayl məzmununu versiya uyğunluğu üçün yeniləyir.
    /// </summary>
    private string UpdateVersionCompatibility(string content, string oldVersion, string newVersion)
    {
        return content.Replace(oldVersion, newVersion);
    }


    /// <summary>
    /// Updates the namespace in the file content.
    /// Fayl məzmununda namespace hissəsini yeniləyir.
    /// </summary>
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


    /// <summary>
    /// Generates the namespace based on the local path.
    /// Yerli yol əsasında namespace yaradır.
    /// </summary>
    private string GenerateNamespace(string localPath, string targetNamespace)
    {
        var entitiesIndex = localPath.IndexOf("Entities", StringComparison.OrdinalIgnoreCase);
        var relativePath = entitiesIndex >= 0 ? localPath[(entitiesIndex + "Entities".Length)..].Trim(Path.DirectorySeparatorChar) : string.Empty;

        relativePath = relativePath.Replace(Path.DirectorySeparatorChar, '.');
        return $"{targetNamespace}.Entities{(string.IsNullOrEmpty(relativePath) ? string.Empty : "." + relativePath)}";
    }
}


