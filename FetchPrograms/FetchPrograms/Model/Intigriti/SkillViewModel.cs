using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class SkillViewModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}