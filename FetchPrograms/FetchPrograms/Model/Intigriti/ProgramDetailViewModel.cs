using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class ProgramDetailViewModel
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("handle")]
    public string Handle { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("following")]
    public bool Following { get; set; }

    [JsonPropertyName("confidentialityLevel")]
    public EnumerationViewModel? ConfidentialityLevel { get; set; }

    [JsonPropertyName("status")]
    public EnumerationViewModel? Status { get; set; }

    [JsonPropertyName("type")]
    public EnumerationViewModel? Type { get; set; }

    [JsonPropertyName("domains")]
    public VersionViewModel<List<DomainViewModel>>? Domains { get; set; }

    [JsonPropertyName("rulesOfEngagement")]
    public VersionWithAttachmentsViewModel<RulesOfEngagementViewModel>? RulesOfEngagement { get; set; }

    [JsonPropertyName("webLinks")]
    public ProgramWebLinks? WebLinks { get; set; }

    [JsonPropertyName("industry")]
    public string? Industry { get; set; }
}