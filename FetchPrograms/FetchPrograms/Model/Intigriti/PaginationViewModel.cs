using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class PaginationViewModel<T>
{
    [JsonPropertyName("maxCount")]
    public int MaxCount { get; set; }

    [JsonPropertyName("records")]
    public List<T> Records { get; set; } = new();
}