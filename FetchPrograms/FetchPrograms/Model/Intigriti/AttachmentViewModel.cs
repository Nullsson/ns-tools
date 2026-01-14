using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class AttachmentViewModel
{
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("code")]
    public int Code { get; set; }
}