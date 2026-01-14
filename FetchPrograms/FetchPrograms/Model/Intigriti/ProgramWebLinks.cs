using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class ProgramWebLinks
{
    [JsonPropertyName("detail")]
    public string Detail { get; set; } = string.Empty;
}