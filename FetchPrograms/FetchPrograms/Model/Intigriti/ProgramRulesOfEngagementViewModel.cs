using System.Text.Json.Serialization;

namespace FetchPrograms.Model.Intigriti;

public class ProgramRulesOfEngagementViewModel
{
    [JsonPropertyName("rulesOfEngagement")]
    public VersionWithAttachmentsViewModel<RulesOfEngagementViewModel>? RulesOfEngagement { get; set; }
}