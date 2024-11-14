using System.Text.Json.Serialization;

namespace GithubSourceCopier.Models;

public class GithubFile
{
    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("download_url")]
    public string DownloadUrl { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; }
}