using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class EnumerationViewModel
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("value")]
    public string Value { get; set; } = string.Empty;
}