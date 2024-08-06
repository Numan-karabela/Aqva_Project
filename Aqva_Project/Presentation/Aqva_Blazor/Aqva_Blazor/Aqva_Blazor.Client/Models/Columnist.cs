using System.Text.Json.Serialization;

namespace Aqva_Blazor.Client.Models;


public sealed class Columnist
{
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;

    [JsonPropertyName("articleTitle")] public string ArticleTitle { get; set; } = string.Empty;

    [JsonPropertyName("publishDate")] public string PublishDate { get; set; } = string.Empty;
}