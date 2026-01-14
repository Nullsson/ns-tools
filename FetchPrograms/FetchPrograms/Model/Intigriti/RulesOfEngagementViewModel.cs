using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class RulesOfEngagementViewModel
{
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("testingRequirements")]
    public TestingRequirementsViewModel? TestingRequirements { get; set; }

    [JsonPropertyName("safeHarbour")]
    public bool SafeHarbour { get; set; }
}