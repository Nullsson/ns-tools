using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class ProgramDomainsViewModel
{
    [JsonPropertyName("domains")]
    public VersionViewModel<List<DomainViewModel>>? Domains { get; set; }
}