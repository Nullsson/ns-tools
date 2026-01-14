using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class PayoutOverviewViewModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public MoneyViewModel? Amount { get; set; }

    [JsonPropertyName("status")]
    public EnumerationViewModel? Status { get; set; }

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("paidAt")]
    public long? PaidAt { get; set; }
}