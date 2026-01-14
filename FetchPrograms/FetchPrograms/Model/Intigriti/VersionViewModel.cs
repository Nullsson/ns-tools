using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class VersionViewModel<T>
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("content")]
    public T? Content { get; set; }
}