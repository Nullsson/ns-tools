using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class ProgramActivityViewModel
{
    [JsonPropertyName("programId")]
    public Guid ProgramId { get; set; }

    [JsonPropertyName("activity")]
    public ActivityViewModel? Activity { get; set; }

    [JsonPropertyName("type")]
    public EnumerationViewModel? Type { get; set; }

    [JsonPropertyName("createdAt")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("following")]
    public bool Following { get; set; }
}