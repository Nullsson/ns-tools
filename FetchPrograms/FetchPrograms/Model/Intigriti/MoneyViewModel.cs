using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class MoneyViewModel
{
    [JsonPropertyName("value")]
    public decimal Value { get; set; }

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
}