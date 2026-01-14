using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class DomainViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("type")]
    public EnumerationViewModel? Type { get; set; }

    [JsonPropertyName("endpoint")]
    public string Endpoint { get; set; } = string.Empty;

    [JsonPropertyName("tier")]
    public EnumerationViewModel? Tier { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("requiredSkills")]
    public List<SkillViewModel> RequiredSkills { get; set; } = new();
}