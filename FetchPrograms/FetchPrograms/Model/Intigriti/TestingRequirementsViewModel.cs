using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class TestingRequirementsViewModel
{
    [JsonPropertyName("intigritiMe")]
    public bool IntigritiMe { get; set; }

    [JsonPropertyName("automatedTooling")]
    public int? AutomatedTooling { get; set; }

    [JsonPropertyName("userAgent")]
    public string? UserAgent { get; set; }

    [JsonPropertyName("requestHeader")]
    public string? RequestHeader { get; set; }
}