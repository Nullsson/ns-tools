using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class VersionWithAttachmentsViewModel<T> : VersionViewModel<T>
{
    [JsonPropertyName("attachments")]
    public List<AttachmentViewModel> Attachments { get; set; } = new();
}